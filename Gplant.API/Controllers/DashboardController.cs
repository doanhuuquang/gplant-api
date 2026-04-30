using Gplant.API.ApiResponse;
using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gplant.API.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    [Authorize(Policy = "AdminOrManager")]
    public class DashboardController(
        IOrderRepository orderRepository,
        IUserService userService,
        IInventoryService inventoryService) : ControllerBase
    {
        /// <summary>
        /// 1. KPI Cards: Tổng quan số liệu cho các thẻ thống kê trên cùng
        /// </summary>
        [HttpGet("overview")]
        public async Task<IActionResult> GetOverviewStats()
        {
            var orderStats = await orderRepository.GetDashboardStatsAsync();
            var userStats = await userService.GetAllUsersAsync(new UserFilterRequest { PageNumber = 1, PageSize = 1 });

            var result = new
            {
                Revenue = new {
                    Today = orderStats.TodayRevenue,
                    // Có thể so sánh với hôm qua để tính % tăng trưởng
                },
                Orders = new {
                    Today = orderStats.TodayOrderCount,
                    Pending = orderStats.PendingCount,
                    Delivering = orderStats.DeliveringCount
                },
                Users = new {
                    Total = userStats.TotalCount,
                    Active = userStats.ActiveUsersCount,
                    Locked = userStats.LockedUsersCount
                }
            };

            return Ok(new SuccessResponse<object>(StatusCodes.Status200OK, "Overview stats retrieved", result, DateTime.UtcNow));
        }

        /// <summary>
        /// 2. Charts: Dữ liệu vẽ biểu đồ doanh thu theo thời gian
        /// </summary>
        [HttpGet("revenue-chart")]
        public async Task<IActionResult> GetRevenueChart([FromQuery] int days = 7)
        {
            var rawData = await orderRepository.GetRevenueChartDataAsync(days);
            
            // Format gọn gàng cho Frontend dễ vẽ biểu đồ (Ví dụ: Chart.js, Recharts, ApexCharts)
            var result = new
            {
                Labels = rawData.Keys.Select(k => k.ToString("dd/MM")).ToList(),
                Data = rawData.Values.ToList()
            };

            return Ok(new SuccessResponse<object>(StatusCodes.Status200OK, "Chart data retrieved", result, DateTime.UtcNow));
        }

        /// <summary>
        /// 3. Recent Activity: Danh sách đơn đặt hàng mới nhất
        /// </summary>
        [HttpGet("recent-orders")]
        public async Task<IActionResult> GetRecentOrders([FromQuery] int limit = 5)
        {
            var orders = await orderRepository.GetRecentOrdersAsync(limit);
            
            var result = orders.Select(o => new {
                o.Id,
                o.OrderNumber,
                o.ShippingName,
                o.Total,
                o.PaymentMethod,
                o.PaymentStatus,
                o.Status,
                CreatedAt = o.CreatedAtUtc
            });

            return Ok(new SuccessResponse<object>(StatusCodes.Status200OK, "Recent orders retrieved", result, DateTime.UtcNow));
        }

        /// <summary>
        /// 4. Alerts: Cảnh báo tồn kho sắp hết để Admin nhập hàng
        /// </summary>
        [HttpGet("low-stock-alerts")]
        public async Task<IActionResult> GetLowStockAlerts([FromQuery] int threshold = 10)
        {
            // Lấy dữ liệu từ service
            var lowStockItems = await inventoryService.GetLowStockItemsAsync(threshold);
            var outOfStockItems = await inventoryService.GetOutOfStockItemsAsync();

            var result = new
            {
                // Select lại để format dữ liệu trả về Frontend. Gắn thêm SKU. 
                // Sử dụng toán tử ?. (null-conditional) để tránh lỗi nếu PlantVariant bị null
                OutOfStock = outOfStockItems.Select(i => new {
                    Sku = i.PlantVariant?.SKU ?? "Unknown SKU",
                    PlantVariantId = i.PlantVariantId,
                    QuantityAvailable = i.QuantityAvailable
                }),
                
                LowStock = lowStockItems.Select(i => new {
                    Sku = i.PlantVariant?.SKU ?? "Unknown SKU",
                    PlantVariantId = i.PlantVariantId,
                    QuantityAvailable = i.QuantityAvailable
                }),
                
                TotalAlerts = outOfStockItems.Count + lowStockItems.Count
            };

            return Ok(new SuccessResponse<object>(StatusCodes.Status200OK, "Inventory alerts retrieved", result, DateTime.UtcNow));
        }
    }
}