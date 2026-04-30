namespace Gplant.Domain.DTOs.Responses
{
    public record CartResponse
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public List<CartItemResponse> Items { get; init; } = [];
        public int TotalItems => Items.Sum(i => i.Quantity);
        public decimal SubTotal => Items.Sum(i => i.Quantity * i.Price);
        public decimal TotalDiscount => Items.Sum(i => i.DiscountAmount);
        public decimal ShippingCost => (SubTotal - TotalDiscount) >= 500000 ? 0 : 50000;
        public decimal Total => SubTotal - TotalDiscount + ShippingCost;
        public DateTimeOffset CreatedAtUtc { get; init; }
        public DateTimeOffset UpdatedAtUtc { get; init; }
    }
}