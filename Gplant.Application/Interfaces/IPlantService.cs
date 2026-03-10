using Gplant.Domain.DTOs.Requests.Plant;
using Gplant.Domain.DTOs.Responses;

namespace Gplant.Application.Interfaces
{
    public interface IPlantService
    {
        Task<PlantResponse> GetByIdAsync(Guid id);
        Task<PlantResponse> GetBySlugAsync(string slug);
        Task<PagedResult<PlantResponse>> GetPlantsAsync(PlantFilterRequest filter);
        Task<List<PlantResponse>> GetPlantsByCategoryAsync(Guid categoryId);
        Task<PlantResponse> CreateAsync(CreatePlantRequest request);
        Task UpdateAsync(Guid id, UpdatePlantRequest request);
        Task DeleteAsync(Guid id);
        Task ToggleActiveAsync(Guid id);
    }
}