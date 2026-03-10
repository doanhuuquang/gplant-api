namespace Gplant.Domain.Entities
{
    public class CartItem
    {
        public Guid Id { get; set; }
        public Guid CartId { get; set; }
        public Guid PlantVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}