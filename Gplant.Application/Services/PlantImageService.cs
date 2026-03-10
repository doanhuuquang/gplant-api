using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.PlantImage;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;
using Gplant.Domain.Exceptions.Media;
using Gplant.Domain.Exceptions.Plant;

namespace Gplant.Application.Services
{
    public class PlantImageService(IPlantImageRepository imageRepository, IPlantRepository plantRepository, IMediaRepository mediaRepository) : IPlantImageService
    {
        public async Task<PlantImageResponse> GetByIdAsync(Guid id)
        {
            var plantImage = await imageRepository.GetByIdAsync(id) ?? throw new PlantException($"Plant image with ID {id} not found");
            return await MapToResponseAsync(plantImage);
        }

        public async Task<List<PlantImageResponse>> GetByPlantIdAsync(Guid plantId)
        {
            _ = await plantRepository.GetByIdAsync(plantId) ?? throw new PlantNotFoundException($"Plant with ID {plantId} not found");
            var plantImages = await imageRepository.GetByPlantIdAsync(plantId);

            var responses = new List<PlantImageResponse>();
            foreach(var plantImage in plantImages)
            {
                responses.Add(await MapToResponseAsync(plantImage));
            }
            
            return responses;
        }

        public async Task<PlantImageResponse> CreateAsync(CreatePlantImageRequest request)
        {
            // Check plant exists
            _ = await plantRepository.GetByIdAsync(request.PlantId) ?? throw new PlantNotFoundException($"Plant with ID {request.PlantId} not found");
            
            // Check media exits 
            if (request.MediaId.HasValue)
            {
                _ = await mediaRepository.GetByIdAsync(request.MediaId.Value) ?? throw new MediaNotFoundException($"Media with ID {request.MediaId} not found");
            }

            // If setting as primary, unset other primary image
            if (request.IsPrimary)
            {
                var existingPrimary = await imageRepository.GetPrimaryImageByPlantIdAsync(request.PlantId);
                if (existingPrimary != null)
                {
                    existingPrimary.IsPrimary = false;
                    await imageRepository.UpdateAsync(existingPrimary);
                }
            }

            var image = new PlantImage
            {
                Id = Guid.NewGuid(),
                PlantId = request.PlantId,
                MediaId = request.MediaId,
                IsPrimary = request.IsPrimary,
            };

            await imageRepository.CreateAsync(image);

            return await MapToResponseAsync(image);
        }

        public async Task DeleteAsync(Guid id)
        {
            var image = await imageRepository.GetByIdAsync(id) ?? throw new PlantException($"Plant image with ID {id} not found");

            await imageRepository.DeleteAsync(image);
        }

        public async Task SetPrimaryImageAsync(Guid imageId)
        {
            var image = await imageRepository.GetByIdAsync(imageId) ?? throw new PlantException($"Plant image with ID {imageId} not found");

            // Unset other primary images for this plant
            var existingPrimary = await imageRepository.GetPrimaryImageByPlantIdAsync(image.PlantId);
            if (existingPrimary != null && existingPrimary.Id != imageId)
            {
                existingPrimary.IsPrimary = false;
                await imageRepository.UpdateAsync(existingPrimary);
            }

            // Set this image as primary
            image.IsPrimary = true;
            image.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await imageRepository.UpdateAsync(image);
        }

        private async Task<PlantImageResponse> MapToResponseAsync(PlantImage plantImage)
        {
            Media? media = null;

            if (plantImage.MediaId.HasValue)
            {
                media = await mediaRepository.GetByIdAsync(plantImage.MediaId.Value);
            }

            return new PlantImageResponse
            {
                Id = plantImage.Id,
                PlantId = plantImage.PlantId,
                Media = media,
                IsPrimary = plantImage.IsPrimary,
                CreatedAtUtc = plantImage.CreatedAtUtc,
                UpdatedAtUtc = plantImage.UpdatedAtUtc
            };
        }
    }
}