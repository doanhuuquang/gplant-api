namespace Gplant.Domain.DTOs.Responses
{
    public record CartResponse
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public List<CartItemResponse> Items { get; init; } = [];
        public int TotalItems => Items.Sum(i => i.Quantity);
        public decimal SubTotal => Items.Sum(i => i.TotalPrice);
        public decimal TotalDiscount => Items.Sum(i => i.DiscountAmount);
        public decimal Total => SubTotal - TotalDiscount;
        public DateTimeOffset CreatedAtUtc { get; init; }
        public DateTimeOffset UpdatedAtUtc { get; init; }
    }
}