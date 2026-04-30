using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.Order;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;
using Gplant.Domain.Enums;
using Gplant.Domain.Exceptions.Cart;
using Gplant.Domain.Exceptions.Inventory;
using Gplant.Domain.Exceptions.Order;
using Gplant.Domain.Exceptions.User;
using Microsoft.AspNetCore.Identity;

namespace Gplant.Application.Services
{
    public class OrderService(
        IOrderRepository orderRepository,
        IOrderItemRepository orderItemRepository,
        ICartService cartService,
        IInventoryService inventoryService,
        IPlantVariantRepository variantRepository,
        IPlantRepository plantRepository,
        IVNPayService vnPayService,      
        IQRPaymentService qrPaymentService,
        IPlantService plantService,
        IPaymentRepository paymentRepository,
        IUserService userService
    ) : IOrderService
    {
        public async Task<OrderResponse> GetByIdAsync(Guid id, Guid? userId = null)
        {
            var order = await orderRepository.GetByIdAsync(id)
                ?? throw new OrderNotFoundException($"Order with ID {id} not found");

            // Chỉ check quyền nếu userId được truyền vào (đi từ luồng Khách Hàng)
            if (userId.HasValue && order.UserId != userId.Value)
                throw new OrderException("You don't have permission to view this order");

            return await MapToResponseAsync(order);
        }

        public async Task<OrderResponse> GetByOrderNumberAsync(string orderNumber, Guid? userId = null)
        {
            var order = await orderRepository.GetByOrderNumberAsync(orderNumber)
                ?? throw new OrderNotFoundException($"Order {orderNumber} not found");

            if (userId.HasValue && order.UserId != userId.Value)
                throw new OrderException("You don't have permission to view this order");

            return await MapToResponseAsync(order);
        }

        public async Task<PagedResult<OrderResponse>> GetMyOrdersAsync(Guid userId, OrderFilterRequest filter)
        {
            var pagedOrders = await orderRepository.GetOrdersAsync(filter, userId);
            
            var responses = new List<OrderResponse>();
            foreach (var order in pagedOrders.Items) responses.Add(await MapToResponseAsync(order));
            
            return new PagedResult<OrderResponse>
            {
                Items = responses,
                TotalCount = pagedOrders.TotalCount,
                PageNumber = pagedOrders.PageNumber,
                PageSize = pagedOrders.PageSize
            };
        }

        public async Task<OrderPagedResult> GetAllOrdersAsync(OrderFilterRequest filter)
        {
            var pagedOrders = await orderRepository.GetOrdersAsync(filter);
            
            // Gọi hàm lấy số liệu thống kê
            var stats = await orderRepository.GetDashboardStatsAsync();
            
            var responses = new List<OrderResponse>();
            foreach (var order in pagedOrders.Items) responses.Add(await MapToResponseAsync(order));
            return new OrderPagedResult
            {
                Items = responses,
                TotalCount = pagedOrders.TotalCount,
                PageNumber = pagedOrders.PageNumber,
                PageSize = pagedOrders.PageSize,
                TodayOrderCount = stats.TodayOrderCount,
                TodayRevenue = stats.TodayRevenue,
                PendingOrderCount = stats.PendingCount,
                DeliveringOrderCount = stats.DeliveringCount,
            };
        }

        public async Task<CreateOrderResponse> CreateOrderAsync(
            Guid userId, CreateOrderRequest request, string? ipAddress = null)
        {
            // Validate shipping info
            if (string.IsNullOrWhiteSpace(request.ShippingName)) throw new OrderException("Shipping name is required");
            if (string.IsNullOrWhiteSpace(request.ShippingPhone)) throw new OrderException("Shipping phone is required");
            if (string.IsNullOrWhiteSpace(request.Address)) throw new OrderException("Address is required");
            if (string.IsNullOrWhiteSpace(request.BuildingName)) throw new OrderException("Building name is required");
            if (string.IsNullOrWhiteSpace(request.Longitude)) throw new OrderException("Longitude is required");
            if (string.IsNullOrWhiteSpace(request.Latitude)) throw new OrderException("Latitude is required");

            // Validate cart
            var cart = await cartService.GetMyCartAsync(userId);
            if (cart.Items.Count == 0) throw new CartException("Cart is empty");

            // Validate stock
            foreach (var item in cart.Items)
            {
                var ok = await inventoryService.CheckStockAvailabilityAsync(item.PlantVariantId, item.Quantity);
                if (!ok) throw new InsufficientStockException($"Insufficient stock for {item.Plant?.Name}");
            }

            // Tạo order entity
            var subTotal = cart.SubTotal;
            var discountAmount = cart.TotalDiscount;
            var shippingFee = cart.ShippingCost;
            var total = subTotal - discountAmount + shippingFee;

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = await GenerateOrderNumberAsync(),
                UserId = userId,
                ShippingName = request.ShippingName,
                ShippingPhone = request.ShippingPhone,
                Address = request.Address,
                BuildingName = request.BuildingName,
                Longitude = request.Longitude,
                Latitude = request.Latitude,
                ShippingNote = request.ShippingNote,
                SubTotal = subTotal,
                DiscountAmount = discountAmount,
                ShippingFee = shippingFee,
                Total = total,
                PaymentMethod = request.PaymentMethod,
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow,
            };

            await orderRepository.CreateAsync(order);

            // Tạo order items + reserve inventory
            var orderItems = new List<OrderItem>();
            foreach (var cartItem in cart.Items)
            {
                var variant = await variantRepository.GetByIdAsync(cartItem.PlantVariantId);
                var plant = variant != null ? await plantRepository.GetByIdAsync(variant.PlantId) : null;

                orderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    PlantId = variant?.PlantId ?? Guid.Empty,
                    PlantVariantId = cartItem.PlantVariantId,
                    PlantName = plant?.Name ?? "Unknown",
                    VariantSKU = variant?.SKU ?? "N/A",
                    VariantSize = variant?.Size ?? 0,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.Price,
                    SalePrice = cartItem.SalePrice,
                    FinalPrice = cartItem.FinalPrice,
                    SubTotal = cartItem.TotalPrice,
                    CreatedAtUtc = DateTimeOffset.UtcNow,
                    UpdatedAtUtc = DateTimeOffset.UtcNow,
                });

                await inventoryService.ReserveInventoryAsync(new Domain.DTOs.Requests.Inventory.ReserveInventoryRequest
                {
                    PlantVariantId = cartItem.PlantVariantId,
                    Quantity = cartItem.Quantity
                });
            }

            await orderItemRepository.CreateBulkAsync(orderItems);
            await cartService.ClearCartAsync(userId);

            return request.PaymentMethod switch
            {
                PaymentMethod.VNPay => await BuildVNPayResponseAsync(order, ipAddress),
                PaymentMethod.BankTransfer => await BuildQRResponseAsync(order),
                _ => await BuildCODResponseAsync(order),
            };
        }

        public async Task HandleVNPayIPNAsync(string orderCode, string responseCode, string transactionId)
        {
            var order = await orderRepository.GetByOrderNumberAsync(orderCode)
                ?? throw new OrderNotFoundException($"Order {orderCode} not found");

            if (order.PaymentStatus == PaymentStatus.Paid) return;

            // Lấy payment Pending mới nhất để update
            var payment = await paymentRepository.GetLatestByOrderIdAsync(order.Id);
            bool isNew = payment == null;

            payment ??= new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Method = PaymentMethod.VNPay,
                Amount = order.Total,
                GatewayName = "VNPay",
                CreatedAtUtc = DateTimeOffset.UtcNow,
            };


            if (responseCode == "00")
            {
                payment.Status = PaymentStatus.Paid;
                payment.GatewayTransactionId = transactionId;
                payment.PaidAtUtc = DateTimeOffset.UtcNow;

                order.PaymentStatus = PaymentStatus.Paid;
                order.PaidAtUtc = DateTimeOffset.UtcNow;
                order.Status = OrderStatus.Confirmed;
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                payment.FailureReason = $"VNPay response code: {responseCode}";

                order.PaymentStatus = PaymentStatus.Failed;
            }

            payment.UpdatedAtUtc = DateTimeOffset.UtcNow;

            if (isNew) await paymentRepository.CreateAsync(payment);
            else await paymentRepository.UpdateAsync(payment);

            await paymentRepository.UpdateAsync(payment);

            order.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await orderRepository.UpdateAsync(order);
        }

        public async Task UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request)
        {
            var order = await orderRepository.GetByIdAsync(orderId)
                ?? throw new OrderNotFoundException($"Order with ID {orderId} not found");

            ValidateStatusTransition(order.Status, request.Status);

            order.Status = request.Status;

            if (request.Status == OrderStatus.Delivered)
            {
                order.PaymentStatus = PaymentStatus.Paid;
                order.PaidAtUtc = DateTimeOffset.UtcNow;
            }

            order.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await orderRepository.UpdateAsync(order);
        }

        public async Task CancelOrderAsync(Guid orderId, Guid userId, CancelOrderRequest request)
        {
            var order = await orderRepository.GetByIdAsync(orderId) ?? throw new OrderNotFoundException($"Order with ID {orderId} not found");

            if (order.UserId != userId) throw new OrderException("You don't have permission to cancel this order");
            if (order.Status is not (OrderStatus.Pending or OrderStatus.Confirmed or OrderStatus.Processing)) throw new InvalidOrderStatusException($"Order with status {order.Status} cannot be cancelled");
            if (string.IsNullOrWhiteSpace(request.Reason)) throw new OrderException("Cancellation reason is required");

            order.Status = OrderStatus.Cancelled;
            order.CancellationReason = request.Reason;
            order.CancelledAtUtc = DateTimeOffset.UtcNow;
            order.UpdatedAtUtc = DateTimeOffset.UtcNow;

            await orderRepository.UpdateAsync(order);

            var orderItems = await orderItemRepository.GetByOrderIdAsync(orderId);
            foreach (var item in orderItems) await inventoryService.ReleaseReservedInventoryAsync(item.PlantVariantId, item.Quantity);
        }

        public async Task<Dictionary<string, object>> GetOrderStatsAsync()
        {
            var todayCount = await orderRepository.GetOrderCountTodayAsync();
            return new Dictionary<string, object> { { "todayOrders", todayCount } };
        }
        
        private async Task<string> GenerateOrderNumberAsync()
        {
            var today = DateTimeOffset.UtcNow;
            var todayCount = await orderRepository.GetOrderCountTodayAsync();
            return $"ORD{today:yyyyMMdd}{(todayCount + 1):D4}";
        }

        private static void ValidateStatusTransition(OrderStatus current, OrderStatus next)
        {
            var valid = new Dictionary<OrderStatus, List<OrderStatus>>
            {
                [OrderStatus.Pending] = [OrderStatus.Confirmed, OrderStatus.Cancelled],
                [OrderStatus.Confirmed] = [OrderStatus.Processing, OrderStatus.Cancelled],
                [OrderStatus.Processing] = [OrderStatus.Shipped, OrderStatus.Cancelled],
                [OrderStatus.Shipped] = [OrderStatus.Delivered, OrderStatus.Cancelled],
                [OrderStatus.Delivered] = [OrderStatus.Refunded],
                [OrderStatus.Cancelled] = [],
                [OrderStatus.Refunded] = [],
            };

            if (!valid[current].Contains(next))
                throw new InvalidOrderStatusException($"Cannot change order status from {current} to {next}");
        }

        private async Task<OrderResponse> MapToResponseAsync(Order order)
        {
            var orderItems = await orderItemRepository.GetByOrderIdAsync(order.Id);
            var user = await userService.GetUserByIdAsync(order.UserId) ?? throw new UserNotExistsException("User not found.");
            var itemResponses = new List<OrderItemResponse>();

            foreach (var item in orderItems) {
                var plant = await plantService.GetByIdAsync(item.PlantId);

                var orderItem = new OrderItemResponse
                {
                    Id = item.Id,
                    OrderId = item.OrderId,
                    PlantVariantId = item.PlantVariantId,
                    Plant = plant,
                    PlantName = item.PlantName,
                    VariantSKU = item.VariantSKU,
                    VariantSize = item.VariantSize,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    SalePrice = item.SalePrice,
                    FinalPrice = item.FinalPrice,
                    SubTotal = item.SubTotal,
                    CreatedAtUtc = item.CreatedAtUtc,
                    UpdatedAtUtc = item.UpdatedAtUtc,
                };

                itemResponses.Add(orderItem);
            }

            return new OrderResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                User = user,
                ShippingName = order.ShippingName,
                ShippingPhone = order.ShippingPhone,
                Address = order.Address,
                BuildingName = order.BuildingName,
                Longitude = order.Longitude,
                Latitude = order.Latitude,
                ShippingNote = order.ShippingNote,
                SubTotal = order.SubTotal,
                DiscountAmount = order.DiscountAmount,
                ShippingFee = order.ShippingFee,
                Total = order.Total,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                PaidAtUtc = order.PaidAtUtc,
                Status = order.Status,
                CancellationReason = order.CancellationReason,
                CancelledAtUtc = order.CancelledAtUtc,
                Items = itemResponses,
                CreatedAtUtc = order.CreatedAtUtc,
                UpdatedAtUtc = order.UpdatedAtUtc,
            };
        }

        private async Task<CreateOrderResponse> BuildCODResponseAsync(Order order)
        {
            var orderResponse = await MapToResponseAsync(order);
            return new CreateOrderResponse
            {
                Order = orderResponse,
                RequiresPayment = false,
            };
        }

        private async Task<CreateOrderResponse> BuildVNPayResponseAsync(Order order, string? clientIp)
        {
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Method = PaymentMethod.VNPay,
                Amount = order.Total,
                Status = PaymentStatus.Pending,
                GatewayName = "VNPay",
                IpAddress = clientIp,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow,
            };
            await paymentRepository.CreateAsync(payment);

            var paymentUrl = vnPayService.CreatePaymentUrl(order, clientIp ?? "127.0.0.1");
            var orderResponse = await MapToResponseAsync(order);
            return new CreateOrderResponse
            {
                Order = orderResponse,
                PaymentUrl = paymentUrl,
                RequiresPayment = true,
                PaymentExpireAtUtc = DateTimeOffset.Now.AddHours(24),
            };
        }

        private async Task<CreateOrderResponse> BuildQRResponseAsync(Order order)
        {
            var qrCodeBase64 = await qrPaymentService.GenerateVietQRCode(order.Total, order.OrderNumber);
            var orderResponse = await MapToResponseAsync(order);
            return new CreateOrderResponse
            {
                Order = orderResponse,
                QrCodeBase64 = qrCodeBase64,
                RequiresPayment = true,
                PaymentExpireAtUtc = DateTimeOffset.UtcNow.AddHours(24),
            };
        }
    }
}