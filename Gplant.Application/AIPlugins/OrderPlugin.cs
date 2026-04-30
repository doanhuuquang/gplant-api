using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.Order;
using Gplant.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json;

namespace Gplant.Application.AIPlugins
{
    public class OrderPlugin(IOrderService orderService)
    {
        private Guid userId;

        public void SetUserId(Guid userId) => this.userId = userId;

        [KernelFunction("get_my_orders")]
        [Description("Get the current user’s order list")]
        public async Task<string> GetMyOrdersAsync()
        {
            try
            {
                if (userId == Guid.Empty) return "Please log in to view your orders.";

                var filter = new OrderFilterRequest { PageNumber = 1, PageSize = 10 };
                var paged = await orderService.GetMyOrdersAsync(userId, filter);

                if (paged.TotalCount == 0) return "You don’t have any orders yet.";

                var json = JsonSerializer.Serialize(new
                {
                    totalCount = paged.TotalCount,
                    orders = paged.Items
                });

                return $"TOOL_DATA:ORDER_LIST:{json}";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        [KernelFunction("get_order_status")]
        [Description("Theo dõi trạng thái đơn hàng qua mã số.")]
        public async Task<string> GetOrderStatusAsync(
            [Description("Mã đơn hàng bắt đầu bằng ORD..., ví dụ: ORD202401050001")]
            string orderNumber)
        {
            try
            {
                var order = await orderService.GetByOrderNumberAsync(orderNumber, userId);
                return $"TOOL_DATA:ORDER_CARD:{JsonSerializer.Serialize(order)}";
            }
            catch (Exception ex)
            {
                return $"Không tìm thấy đơn hàng {orderNumber}: {ex.Message}";
            }
        }
    }
}