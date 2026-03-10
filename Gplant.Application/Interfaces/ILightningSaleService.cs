using Gplant.Domain.DTOs.Requests.LightningSale;
using Gplant.Domain.DTOs.Responses;

namespace Gplant.Application.Interfaces
{
    public interface ILightningSaleService
    {
        Task<LightningSaleResponse> GetByIdAsync(Guid id);
        Task<List<LightningSaleResponse>> GetAllAsync();
        Task<List<LightningSaleResponse>> GetActiveAsync();
        Task<List<LightningSaleResponse>> GetUpcomingAsync();
        Task<List<LightningSaleResponse>> GetOngoingAsync();
        Task<LightningSaleResponse?> GetCurrentActiveSaleAsync();
        Task<LightningSaleResponse> CreateAsync(CreateLightningSaleRequest request);
        Task UpdateAsync(Guid id, UpdateLightningSaleRequest request);
        Task DeleteAsync(Guid id);
        Task ActivateAsync(Guid id);
        Task DeactivateAsync(Guid id);
        Task<LightningSaleItemResponse> AddItemAsync(Guid saleId, AddSaleItemRequest request);
        Task UpdateItemAsync(Guid itemId, UpdateSaleItemRequest request);
        Task RemoveItemAsync(Guid itemId);
        Task<LightningSaleItemResponse?> GetLightningSaleItemByVariantAsync(Guid plantVariantId);
        Task IncrementSoldQuantityAsync(Guid itemId, int quantity);
    }
}