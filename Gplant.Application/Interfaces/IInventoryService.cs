using Gplant.Domain.DTOs.Requests.Inventory;
using Gplant.Domain.DTOs.Responses;

namespace Gplant.Application.Interfaces
{
    public interface IInventoryService
    {
        Task<InventoryResponse> GetByIdAsync(Guid id);
        Task<InventoryResponse> GetByPlantVariantIdAsync(Guid plantVariantId);
        Task<List<InventoryResponse>> GetAllAsync();
        Task<List<InventoryResponse>> GetLowStockItemsAsync(int threshold = 10);
        Task<List<InventoryResponse>> GetOutOfStockItemsAsync();
        Task<InventoryResponse> CreateAsync(CreateInventoryRequest request);
        Task UpdateAsync(Guid id, UpdateInventoryRequest request);
        Task DeleteAsync(Guid id);
        Task AdjustInventoryAsync(Guid id, AdjustInventoryRequest request);
        Task ReserveInventoryAsync(ReserveInventoryRequest request);
        Task ReleaseReservedInventoryAsync(Guid plantVariantId, int quantity);
        Task<bool> CheckStockAvailabilityAsync(Guid plantVariantId, int quantity);
    }
}