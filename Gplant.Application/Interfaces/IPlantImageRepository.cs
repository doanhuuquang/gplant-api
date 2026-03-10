using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;

namespace Gplant.Application.Interfaces
{
    public interface IPlantImageRepository
    {
        Task<PlantImage?> GetByIdAsync(Guid id);
        Task<List<PlantImage>> GetByPlantIdAsync(Guid plantId);
        Task<PlantImage?> GetPrimaryImageByPlantIdAsync(Guid plantId);
        Task CreateAsync(PlantImage image);
        Task UpdateAsync(PlantImage image);
        Task DeleteAsync(PlantImage image);
    }
}