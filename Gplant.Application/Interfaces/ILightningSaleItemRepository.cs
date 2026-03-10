using Gplant.Domain.Entities;

namespace Gplant.Application.Interfaces
{
    public interface ILightningSaleItemRepository
    {
        Task<LightningSaleItem?> GetByIdAsync(Guid id);
        Task<List<LightningSaleItem>> GetBySaleIdAsync(Guid saleId);
        Task<LightningSaleItem?> GetByVariantIdAsync(Guid plantVariantId, Guid? saleId = null);
        Task<List<LightningSaleItem>> GetActiveItemsAsync();
        Task CreateAsync(LightningSaleItem item);
        Task UpdateAsync(LightningSaleItem item);
        Task DeleteAsync(LightningSaleItem item);
    }
}