namespace Gplant.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 0,        // Chờ xác nhận
        Confirmed = 1,      // Đã xác nhận
        Processing = 2,     // Đang xử lý
        Shipped = 3,        // Đang giao hàng
        Delivered = 4,      // Đã giao
        Cancelled = 5,      // Đã hủy
        Refunded = 6        // Đã hoàn tiền
    }
}