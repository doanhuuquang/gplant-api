using Gplant.Domain.DTOs.Requests.PlantImage;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;

namespace Gplant.Application.Interfaces
{
    public interface IPlantImageService
    {
        Task<PlantImageResponse> GetByIdAsync(Guid id);
        Task<List<PlantImageResponse>> GetByPlantIdAsync(Guid plantId);
        Task<PlantImageResponse> CreateAsync(CreatePlantImageRequest request);
        Task DeleteAsync(Guid id);
        Task SetPrimaryImageAsync(Guid imageId);
    }
}