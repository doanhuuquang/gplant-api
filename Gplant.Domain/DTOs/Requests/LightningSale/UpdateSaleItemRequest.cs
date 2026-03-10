namespace Gplant.Domain.DTOs.Requests.LightningSale
{
    public record UpdateSaleItemRequest
    {
        public decimal? SalePrice { get; init; }
        public int? QuantityLimit { get; init; }
        public bool? IsActive { get; init; }
    }
}