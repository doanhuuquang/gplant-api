using Gplant.Application.Interfaces;
using Gplant.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class OrderTimeoutBackgroundService(IServiceProvider serviceProvider) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            var orderItemRepository = scope.ServiceProvider.GetRequiredService<IOrderItemRepository>();
            var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

            var now = DateTimeOffset.UtcNow;
            var timeoutBankTransfer = now.AddDays(-1);
            var timeoutVNPay = now.AddMinutes(-15);

            // Lấy các đơn hàng chưa thanh toán quá 1 ngày đối với phương thức thanh toán bank transfer, quá 15 phút đối với phương thức thanh toán vnpay
            var expiredOrdersBankTransfer = await orderRepository.GetOrdersTimeoutAsync(PaymentMethod.BankTransfer, timeoutBankTransfer);
            var expiredOrdersVNPay = await orderRepository.GetOrdersTimeoutAsync(PaymentMethod.VNPay, timeoutVNPay);

            var expiredOrders = expiredOrdersBankTransfer.Concat(expiredOrdersVNPay).ToList();

            foreach (var order in expiredOrders)
            {
                order.Status = OrderStatus.Cancelled;
                order.PaymentStatus = PaymentStatus.Failed;
                order.CancellationReason = "Payment timeout";
                order.CancelledAtUtc = now;
                order.UpdatedAtUtc = now;
                await orderRepository.UpdateAsync(order);

                // Giải phóng inventory
                var orderItems = await orderItemRepository.GetByOrderIdAsync(order.Id);
                foreach (var item in orderItems)
                {
                    await inventoryService.ReleaseReservedInventoryAsync(item.PlantVariantId, item.Quantity);
                }
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}