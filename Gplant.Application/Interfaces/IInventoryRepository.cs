using Gplant.Domain.Entities;

namespace Gplant.Application.Interfaces
{
    public interface IInventoryRepository
    {
        Task<Inventory?> GetByIdAsync(Guid id);
        Task<Inventory?> GetByPlantVariantIdAsync(Guid plantVariantId);
        Task<List<Inventory>> GetAllAsync();
        Task<List<Inventory>> GetLowStockItemsAsync(int threshold = 10);
        Task<List<Inventory>> GetOutOfStockItemsAsync();
        Task CreateAsync(Inventory inventory);
        Task UpdateAsync(Inventory inventory);
        Task DeleteAsync(Inventory inventory);
    }
}