using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.Email;
using Gplant.Domain.DTOs.Requests.Order;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;
using Gplant.Domain.Enums;
using Gplant.Domain.Exceptions.Cart;
using Gplant.Domain.Exceptions.Inventory;
using Gplant.Domain.Exceptions.Order;

namespace Gplant.Application.Services
{
    public class OrderService(
        IOrderRepository orderRepository,
        IOrderItemRepository orderItemRepository,
        ICartService cartService,
        IInventoryService inventoryService,
        IPlantVariantRepository variantRepository,
        IPlantRepository plantRepository,
        IEmailProcessor emailProcessor,
        IPaymentService paymentService) : IOrderService
    {
        public async Task<OrderResponse> GetByIdAsync(Guid id, Guid userId)
        {
            var order = await orderRepository.GetByIdAsync(id) ?? throw new OrderNotFoundException($"Order with ID {id} not found");

            // Verify ownership
            if (order.UserId != userId) throw new OrderException("You don't have permission to view this order");

            return await MapToResponseAsync(order);
        }

        public async Task<OrderResponse> GetByOrderNumberAsync(string orderNumber, Guid userId)
        {
            var order = await orderRepository.GetByOrderNumberAsync(orderNumber) ?? throw new OrderNotFoundException($"Order {orderNumber} not found");

            // Verify ownership
            if (order.UserId != userId) throw new OrderException("You don't have permission to view this order");

            return await MapToResponseAsync(order);
        }

        public async Task<PagedResult<OrderResponse>> GetMyOrdersAsync(Guid userId, OrderFilterRequest filter)
        {
            var pagedOrders = await orderRepository.GetOrdersAsync(filter, userId);

            var orderResponses = new List<OrderResponse>();
            foreach (var order in pagedOrders.Items)
            {
                orderResponses.Add(await MapToResponseAsync(order));
            }

            return new PagedResult<OrderResponse>
            {
                Items = orderResponses,
                TotalCount = pagedOrders.TotalCount,
                PageNumber = pagedOrders.PageNumber,
                PageSize = pagedOrders.PageSize
            };
        }

        public async Task<PagedResult<OrderResponse>> GetAllOrdersAsync(OrderFilterRequest filter)
        {
            var pagedOrders = await orderRepository.GetOrdersAsync(filter);

            var orderResponses = new List<OrderResponse>();
            foreach (var order in pagedOrders.Items)
            {
                orderResponses.Add(await MapToResponseAsync(order));
            }

            return new PagedResult<OrderResponse>
            {
                Items = orderResponses,
                TotalCount = pagedOrders.TotalCount,
                PageNumber = pagedOrders.PageNumber,
                PageSize = pagedOrders.PageSize
            };
        }

        public async Task<CreateOrderResponse> CreateOrderAsync(Guid userId, CreateOrderRequest request, string? ipAddress = null)
        {
            // Validate shipping info
            if (string.IsNullOrWhiteSpace(request.ShippingName)) throw new OrderException("Shipping name is required");
            if (string.IsNullOrWhiteSpace(request.ShippingPhone)) throw new OrderException("Shipping phone is required");
            if (string.IsNullOrWhiteSpace(request.ShippingAddress)) throw new OrderException("Shipping address is required");
            if (string.IsNullOrWhiteSpace(request.ShippingWard)) throw new OrderException("Shipping ward is required");
            if (string.IsNullOrWhiteSpace(request.ShippingDistrict)) throw new OrderException("Shipping district is required");
            if (string.IsNullOrWhiteSpace(request.ShippingProvince)) throw new OrderException("Shipping province is required");

            // Get cart
            var cart = await cartService.GetMyCartAsync(userId);
            if (cart.Items.Count == 0) throw new CartException("Cart is empty");

            // Validate stock for all items
            foreach (var item in cart.Items)
            {
                var isAvailable = await inventoryService.CheckStockAvailabilityAsync(
                    item.PlantVariantId, 
                    item.Quantity);

                if (!isAvailable) throw new InsufficientStockException($"Insufficient stock for {item.Plant?.Name}");
            }

            // Generate order number
            var orderNumber = await GenerateOrderNumberAsync();

            // Calculate totals
            var subTotal = cart.SubTotal;
            var discountAmount = cart.TotalDiscount;
            var shippingFee = CalculateShippingFee(request.ShippingProvince);
            var total = subTotal - discountAmount + shippingFee;

            // Create order
            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = orderNumber,
                UserId = userId,
                ShippingName = request.ShippingName,
                ShippingPhone = request.ShippingPhone,
                ShippingEmail = request.ShippingEmail,
                ShippingAddress = request.ShippingAddress,
                ShippingWard = request.ShippingWard,
                ShippingDistrict = request.ShippingDistrict,
                ShippingProvince = request.ShippingProvince,
                ShippingNote = request.ShippingNote,
                SubTotal = subTotal,
                DiscountAmount = discountAmount,
                ShippingFee = shippingFee,
                Total = total,
                PaymentMethod = request.PaymentMethod,
                PaymentStatus = request.PaymentMethod switch
                {
                    PaymentMethod.COD => PaymentStatus.Pending, // COD đợi khi giao hàng
                    PaymentMethod.BankTransfer => PaymentStatus.Pending, // Chờ chuyển khoản
                    _ => PaymentStatus.AwaitingPayment // Online payment (VNPay, MoMo, ZaloPay)
                },
                Status = OrderStatus.Pending,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            await orderRepository.CreateAsync(order);

            // Create order items
            var orderItems = new List<OrderItem>();
            foreach (var cartItem in cart.Items)
            {
                var variant = await variantRepository.GetByIdAsync(cartItem.PlantVariantId);
                var plant = variant != null ? await plantRepository.GetByIdAsync(variant.PlantId) : null;

                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    PlantVariantId = cartItem.PlantVariantId,
                    PlantName = plant?.Name ?? "Unknown",
                    VariantSKU = variant?.SKU ?? "N/A",
                    VariantSize = variant?.Size ?? 0,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.Price,
                    SalePrice = cartItem.SalePrice,
                    FinalPrice = cartItem.FinalPrice,
                    SubTotal = cartItem.TotalPrice,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow
                };

                orderItems.Add(orderItem);

                // Reserve inventory
                await inventoryService.ReserveInventoryAsync(new Domain.DTOs.Requests.Inventory.ReserveInventoryRequest
                {
                    PlantVariantId = cartItem.PlantVariantId,
                    Quantity = cartItem.Quantity
                });
            }

            await orderItemRepository.CreateBulkAsync(orderItems);

            // Clear cart
            await cartService.ClearCartAsync(userId);

            // Generate payment URL for online payment
            string? paymentUrl = null;
            var requiresPayment = request.PaymentMethod != PaymentMethod.COD && request.PaymentMethod != PaymentMethod.BankTransfer;

            if (requiresPayment)
            {
                var returnUrl = $"https://yourdomain.com/payment/callback";
                paymentUrl = await paymentService.CreatePaymentUrlAsync(order, returnUrl, ipAddress ?? "127.0.0.1");
                
                order.PaymentAttemptedAtUtc = DateTime.UtcNow;
                await orderRepository.UpdateAsync(order);
            }
            else
            {
                // Send order confirmation email for COD/Bank Transfer
                await SendOrderConfirmationEmailAsync(order);
            }

            var orderResponse = await MapToResponseAsync(order);

            return new CreateOrderResponse
            {
                Order = orderResponse,
                PaymentUrl = paymentUrl,
                RequiresPayment = requiresPayment
            };
        }

        public async Task UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request)
        {
            var order = await orderRepository.GetByIdAsync(orderId)
                ?? throw new OrderNotFoundException($"Order with ID {orderId} not found");

            // Validate status transition
            ValidateStatusTransition(order.Status, request.Status);

            order.Status = request.Status;

            // Handle specific status changes
            if (request.Status == OrderStatus.Delivered)
            {
                order.PaymentStatus = PaymentStatus.Paid;
                order.PaidAtUtc = DateTime.UtcNow;
            }

            await orderRepository.UpdateAsync(order);

            // Send status update email
            await SendOrderStatusUpdateEmailAsync(order);
        }

        public async Task CancelOrderAsync(Guid orderId, Guid userId, CancelOrderRequest request)
        {
            var order = await orderRepository.GetByIdAsync(orderId)
                ?? throw new OrderNotFoundException($"Order with ID {orderId} not found");

            // Verify ownership
            if (order.UserId != userId)
                throw new OrderException("You don't have permission to cancel this order");

            // Check if order can be cancelled
            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed && order.Status != OrderStatus.Processing)
                throw new InvalidOrderStatusException($"Order with status {order.Status} cannot be cancelled");

            if (string.IsNullOrWhiteSpace(request.Reason))
                throw new OrderException("Cancellation reason is required");

            order.Status = OrderStatus.Cancelled;
            order.CancellationReason = request.Reason;
            order.CancelledAtUtc = DateTime.UtcNow;

            await orderRepository.UpdateAsync(order);

            // Release reserved inventory
            var orderItems = await orderItemRepository.GetByOrderIdAsync(orderId);
            foreach (var item in orderItems)
            {
                await inventoryService.ReleaseReservedInventoryAsync(
                    item.PlantVariantId, 
                    item.Quantity);
            }

            // Send cancellation email
            await SendOrderCancellationEmailAsync(order);
        }

        public async Task<Dictionary<string, object>> GetOrderStatsAsync()
        {
            var todayCount = await orderRepository.GetOrderCountTodayAsync();
            
            // You can add more stats here
            return new Dictionary<string, object>
            {
                { "todayOrders", todayCount }
            };
        }

        private async Task<string> GenerateOrderNumberAsync()
        {
            var today = DateTime.UtcNow;
            var todayCount = await orderRepository.GetOrderCountTodayAsync();
            var orderNumber = $"ORD{today:yyyyMMdd}{(todayCount + 1):D4}";
            return orderNumber;
        }

        private static decimal CalculateShippingFee(string province)
        {
            // Simple shipping fee calculation
            // You can make this more complex based on your business logic
            var normalizedProvince = province.ToLower();

            if (normalizedProvince.Contains("hà nội") || normalizedProvince.Contains("hồ chí minh"))
                return 30000; // 30k for major cities

            return 50000; // 50k for other provinces
        }

        private static void ValidateStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            var validTransitions = new Dictionary<OrderStatus, List<OrderStatus>>
            {
                { OrderStatus.Pending, new List<OrderStatus> { OrderStatus.Confirmed, OrderStatus.Cancelled } },
                { OrderStatus.Confirmed, new List<OrderStatus> { OrderStatus.Processing, OrderStatus.Cancelled } },
                { OrderStatus.Processing, new List<OrderStatus> { OrderStatus.Shipped, OrderStatus.Cancelled } },
                { OrderStatus.Shipped, new List<OrderStatus> { OrderStatus.Delivered, OrderStatus.Cancelled } }, // ⚠️ SỬA: Cho phép cancel khi đang ship
                { OrderStatus.Delivered, new List<OrderStatus> { OrderStatus.Refunded } },
                { OrderStatus.Cancelled, new List<OrderStatus>() },
                { OrderStatus.Refunded, new List<OrderStatus>() }
            };

            if (!validTransitions[currentStatus].Contains(newStatus))
            {
                throw new InvalidOrderStatusException(
                    $"Cannot change order status from {currentStatus} to {newStatus}");
            }
        }

        private async Task SendOrderConfirmationEmailAsync(Order order)
        {
            var emailRequest = new EmailRequest
            {
                Receptor = order.ShippingEmail ?? "customer@example.com", // Cần thêm email vào Order entity
                Subject = $"Xác nhận đơn hàng - {order.OrderNumber}",
                Body = $@"
                    Xin chào {order.ShippingName},

                    Cảm ơn bạn đã đặt hàng tại Gplant!

                    Thông tin đơn hàng:
                    ━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                    Mã đơn hàng: {order.OrderNumber}
                    Ngày đặt: {order.CreatedAtUtc:dd/MM/yyyy HH:mm}

                    Địa chỉ giao hàng:
                    {order.ShippingName} - {order.ShippingPhone}
                    {order.ShippingAddress}, {order.ShippingWard}, {order.ShippingDistrict}, {order.ShippingProvince}

                    Tổng tiền hàng: {order.SubTotal:N0} VND
                    Giảm giá: -{order.DiscountAmount:N0} VND
                    Phí vận chuyển: {order.ShippingFee:N0} VND
                    ━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                    TỔNG THANH TOÁN: {order.Total:N0} VND

                    Phương thức thanh toán: {order.PaymentMethod}

                    Chúng tôi sẽ xử lý đơn hàng của bạn trong thời gian sớm nhất.

                    Trân trọng,
                    Gplant Team
                "
            };

            await emailProcessor.SendEmail(emailRequest);
        }

        private async Task SendOrderStatusUpdateEmailAsync(Order order)
        {
            var statusMessage = order.Status switch
            {
                OrderStatus.Confirmed => "Đơn hàng của bạn đã được xác nhận và đang được chuẩn bị.",
                OrderStatus.Processing => "Đơn hàng của bạn đang được xử lý.",
                OrderStatus.Shipped => "Đơn hàng của bạn đã được giao cho đơn vị vận chuyển.",
                OrderStatus.Delivered => "Đơn hàng của bạn đã được giao thành công. Cảm ơn bạn đã mua hàng!",
                OrderStatus.Refunded => "Đơn hàng của bạn đã được hoàn tiền.",
                _ => "Trạng thái đơn hàng của bạn đã được cập nhật."
            };

            var emailRequest = new EmailRequest
            {
                Receptor = order.ShippingEmail ?? "customer@example.com",
                Subject = $"Cập nhật đơn hàng #{order.OrderNumber}",
                Body = $@"
                    Xin chào {order.ShippingName},

                    {statusMessage}

                    Thông tin đơn hàng:
                    ━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                    Mã đơn hàng: {order.OrderNumber}
                    Trạng thái: {order.Status}
                    Tổng tiền: {order.Total:N0} VND

                    Bạn có thể theo dõi đơn hàng tại trang web của chúng tôi.

                    Trân trọng,
                    Gplant Team
                "
            };

            await emailProcessor.SendEmail(emailRequest);
        }

        private async Task SendOrderCancellationEmailAsync(Order order)
        {
            var emailRequest = new EmailRequest
            {
                Receptor = order.ShippingEmail ?? "customer@example.com",
                Subject = $"Đơn hàng đã bị hủy - {order.OrderNumber}",
                Body = $@"
                    Xin chào {order.ShippingName},

                    Đơn hàng #{order.OrderNumber} của bạn đã bị hủy.

                    Thông tin hủy đơn:
                    ━━━━━━━━━━━━━━━━━━━━━━━━━━━━
                    Mã đơn hàng: {order.OrderNumber}
                    Lý do hủy: {order.CancellationReason}
                    Ngày hủy: {order.CancelledAtUtc:dd/MM/yyyy HH:mm}
                    Số tiền: {order.Total:N0} VND

                    {(order.PaymentStatus == PaymentStatus.Paid ? "Số tiền sẽ được hoàn lại trong vòng 7-10 ngày làm việc." : "")}

                    Nếu có bất kỳ thắc mắc nào, vui lòng liên hệ với chúng tôi.

                    Trân trọng,
                    Gplant Team
                "
            };

            await emailProcessor.SendEmail(emailRequest);
        }

        private async Task<OrderResponse> MapToResponseAsync(Order order)
        {
            var orderItems = await orderItemRepository.GetByOrderIdAsync(order.Id);

            var itemResponses = orderItems.Select(item => new OrderItemResponse
            {
                Id = item.Id,
                OrderId = item.OrderId,
                PlantVariantId = item.PlantVariantId,
                PlantName = item.PlantName,
                VariantSKU = item.VariantSKU,
                VariantSize = item.VariantSize,
                Quantity = item.Quantity,
                Price = item.Price,
                SalePrice = item.SalePrice,
                FinalPrice = item.FinalPrice,
                SubTotal = item.SubTotal,
                CreatedAtUtc = item.CreatedAtUtc,
                UpdatedAtUtc = item.UpdatedAtUtc
            }).ToList();

            return new OrderResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                ShippingName = order.ShippingName,
                ShippingPhone = order.ShippingPhone,
                ShippingEmail = order.ShippingEmail,
                ShippingAddress = order.ShippingAddress,
                ShippingWard = order.ShippingWard,
                ShippingDistrict = order.ShippingDistrict,
                ShippingProvince = order.ShippingProvince,
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
                UpdatedAtUtc = order.UpdatedAtUtc
            };
        }

        // Thêm method xử lý payment callback
        public async Task<PaymentCallbackResponse> ProcessPaymentCallbackAsync(Dictionary<string, string> queryParams)
        {
            var callbackResponse = await paymentService.ProcessPaymentCallbackAsync(queryParams);

            var order = await orderRepository.GetByOrderNumberAsync(callbackResponse.OrderNumber);
            if (order == null)
            {
                return callbackResponse with { Success = false, Message = "Order not found" };
            }

            // Validate payment amount
            if (callbackResponse.Amount != order.Total)
            {
                return callbackResponse with 
                { 
                    Success = false, 
                    Message = "Payment amount mismatch" 
                };
            }

            // Check payment status
            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                return callbackResponse with 
                { 
                    Success = true, 
                    Message = "Order already paid" 
                };
            }

            order.PaymentStatus = callbackResponse.PaymentStatus;
            order.PaymentTransactionId = callbackResponse.TransactionId;
            order.PaymentGatewayResponse = System.Text.Json.JsonSerializer.Serialize(queryParams); // ⚠️ THÊM
            
            if (callbackResponse.Success)
            {
                order.PaidAtUtc = DateTime.UtcNow;
                order.Status = OrderStatus.Confirmed;
                await SendOrderConfirmationEmailAsync(order);
            }
            else
            {
                // Xử lý payment failed
                order.Status = OrderStatus.Cancelled;
                order.CancellationReason = $"Payment failed: {callbackResponse.Message}";
                order.CancelledAtUtc = DateTime.UtcNow;
                
                // Release inventory
                var orderItems = await orderItemRepository.GetByOrderIdAsync(order.Id);
                foreach (var item in orderItems)
                {
                    await inventoryService.ReleaseReservedInventoryAsync(
                        item.PlantVariantId, 
                        item.Quantity);
                }
            }

            await orderRepository.UpdateAsync(order);

            return callbackResponse;
        }

        // Thêm method kiểm tra payment timeout
        public async Task CheckPaymentTimeoutAsync()
        {
            var timeoutMinutes = 15; // VNPay timeout 15 phút
            var cutoffTime = DateTime.UtcNow.AddMinutes(-timeoutMinutes);
            
            var timeoutOrders = await orderRepository.GetOrdersWithPaymentTimeoutAsync(cutoffTime);
            
            foreach (var order in timeoutOrders)
            {
                if (order.PaymentStatus == PaymentStatus.AwaitingPayment)
                {
                    order.Status = OrderStatus.Cancelled;
                    order.PaymentStatus = PaymentStatus.Failed;
                    order.CancellationReason = "Payment timeout";
                    order.CancelledAtUtc = DateTime.UtcNow;
                    
                    await orderRepository.UpdateAsync(order);
                    
                    // Release inventory
                    var orderItems = await orderItemRepository.GetByOrderIdAsync(order.Id);
                    foreach (var item in orderItems)
                    {
                        await inventoryService.ReleaseReservedInventoryAsync(
                            item.PlantVariantId, 
                            item.Quantity);
                    }
                }
            }
        }

        // Thêm method xác nhận đã chuyển khoản
        public async Task ConfirmBankTransferAsync(Guid orderId, string transactionId, string? note)
        {
            var order = await orderRepository.GetByIdAsync(orderId)
                ?? throw new OrderNotFoundException($"Order with ID {orderId} not found");

            if (order.PaymentMethod != PaymentMethod.BankTransfer)
                throw new OrderException("This order is not bank transfer payment");

            if (order.PaymentStatus != PaymentStatus.Pending)
                throw new OrderException("Order payment already processed");

            order.PaymentStatus = PaymentStatus.Paid;
            order.PaymentTransactionId = transactionId;
            order.PaidAtUtc = DateTime.UtcNow;
            order.Status = OrderStatus.Confirmed;

            await orderRepository.UpdateAsync(order);
            await SendOrderConfirmationEmailAsync(order);
        }
    }
}