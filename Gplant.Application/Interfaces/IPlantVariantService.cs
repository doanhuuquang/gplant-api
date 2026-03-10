using Gplant.Domain.DTOs.Requests.PlantVariant;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;

namespace Gplant.Application.Interfaces
{
    public interface IPlantVariantService
    {
        Task<PlantVariantResponse> GetByIdAsync(Guid id);
        Task<List<PlantVariantResponse>> GetByPlantIdAsync(Guid plantId);
        Task<PlantVariantResponse> CreateAsync(CreatePlantVariantRequest request);
        Task UpdateAsync(Guid id, UpdatePlantVariantRequest request);
        Task DeleteAsync(Guid id);
        Task ToggleActiveAsync(Guid id);
    }
}