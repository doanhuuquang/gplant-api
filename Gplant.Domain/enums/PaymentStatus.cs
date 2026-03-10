namespace Gplant.Domain.Enums
{
    public enum PaymentStatus
    {
        Pending = 0,            // Chờ thanh toán
        AwaitingPayment = 1,    // Đang chờ thanh toán online
        Paid = 2,               // Đã thanh toán
        Failed = 3,             // Thanh toán thất bại
        Refunded = 4            // Đã hoàn tiền
    }
}