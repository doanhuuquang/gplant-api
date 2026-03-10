using Gplant.Domain.DTOs.Requests.Plant;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;

namespace Gplant.Application.Interfaces
{
    public interface IPlantRepository
    {
        Task<Plant?> GetByIdAsync(Guid id);
        Task<Plant?> GetBySlugAsync(string slug);
        Task<PagedResult<Plant>> GetPlantsAsync(PlantFilterRequest filter);
        Task<List<Plant>> GetPlantsByCategoryAsync(Guid categoryId);
        Task CreateAsync(Plant plant);
        Task UpdateAsync(Plant plant);
        Task DeleteAsync(Plant plant);
        Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null);
        Task<bool> HasVariantsAsync(Guid plantId);
        Task<bool> IsUsedInOrdersAsync(Guid plantId);
    }
}