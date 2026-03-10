using Gplant.Domain.Entities;

namespace Gplant.Application.Interfaces
{
    public interface IPlantVariantRepository
    {
        Task<PlantVariant?> GetByIdAsync(Guid id);
        Task<PlantVariant?> GetBySKUAsync(string sku);
        Task<List<PlantVariant>> GetByPlantIdAsync(Guid plantId);
        Task CreateAsync(PlantVariant variant);
        Task UpdateAsync(PlantVariant variant);
        Task DeleteAsync(PlantVariant variant);
        Task<bool> SKUExistsAsync(string sku, Guid? excludeId = null);
        Task<bool> IsUsedInOrdersAsync(Guid variantId);
        Task<bool> IsUsedInCartsAsync(Guid variantId);
        Task<bool> HasInventoryAsync(Guid variantId);
    }
}