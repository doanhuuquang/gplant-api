using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.PlantVariant;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;
using Gplant.Domain.Exceptions.Plant;
using Gplant.Domain.Exceptions.PlantVariant;

namespace Gplant.Application.Services
{
    public class PlantVariantService(
        IPlantVariantRepository variantRepository,
        IPlantRepository plantRepository) : IPlantVariantService
    {
        public async Task<PlantVariantResponse> GetByIdAsync(Guid id)
        {
            var plantVariant = await variantRepository.GetByIdAsync(id) ?? throw new PlantVariantException($"Plant variant with ID {id} not found");
            return await MapToResponseAsync(plantVariant);
        }

        public async Task<List<PlantVariantResponse>> GetByPlantIdAsync(Guid plantId)
        {
            var plantVariants = await variantRepository.GetByPlantIdAsync(plantId);

            var plantVariantResponses = new List<PlantVariantResponse>();
            foreach (var plantVariant in plantVariants)
            {
                plantVariantResponses.Add(await MapToResponseAsync(plantVariant));
            }

            return plantVariantResponses;
        }

        public async Task<PlantVariantResponse> CreateAsync(CreatePlantVariantRequest request)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(request.SKU)) throw new PlantVariantException("SKU is required");
            if (request.Price <= 0) throw new PlantVariantException("Price must be greater than 0");
            if (request.Size <= 0) throw new PlantVariantException("Size must be greater than 0");

            // Check plant exists
            _ = await plantRepository.GetByIdAsync(request.PlantId) ?? throw new PlantNotFoundException($"Plant with ID {request.PlantId} not found");

            // Check SKU unique
            if (await variantRepository.SKUExistsAsync(request.SKU)) throw new PlantVariantException($"SKU '{request.SKU}' already exists");

            var variant = new PlantVariant
            {
                Id = Guid.NewGuid(),
                PlantId = request.PlantId,
                SKU = request.SKU,
                Price = request.Price,
                Size = request.Size,
                IsActive = request.IsActive,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            };

            await variantRepository.CreateAsync(variant);

            return await MapToResponseAsync(variant);
        }

        public async Task UpdateAsync(Guid id, UpdatePlantVariantRequest request)
        {
            var variant = await variantRepository.GetByIdAsync(id) ?? throw new PlantVariantException($"Plant variant with ID {id} not found");

            if (!string.IsNullOrWhiteSpace(request.SKU) && request.SKU != variant.SKU)
            {
                if (await variantRepository.SKUExistsAsync(request.SKU, variant.Id)) throw new PlantVariantException($"SKU '{request.SKU}' already exists");
                variant.SKU = request.SKU;
            }

            if (request.Price.HasValue)
            {
                if (request.Price.Value <= 0) throw new PlantVariantException("Price must be greater than 0");
                variant.Price = request.Price.Value;
            }

            if (request.Size.HasValue)
            {
                if (request.Size.Value <= 0) throw new PlantVariantException("Size must be greater than 0");
                variant.Size = request.Size.Value;
            }

            if (request.IsActive.HasValue) variant.IsActive = request.IsActive.Value;

            variant.UpdatedAtUtc = DateTimeOffset.UtcNow;

            await variantRepository.UpdateAsync(variant);
        }

        public async Task DeleteAsync(Guid id)
        {
            var variant = await variantRepository.GetByIdAsync(id)
                ?? throw new PlantVariantException($"Plant variant with ID {id} not found");

            // ✅ Kiểm tra có trong đơn hàng không
            var isUsedInOrders = await variantRepository.IsUsedInOrdersAsync(id);
            if (isUsedInOrders) throw new PlantVariantException("Cannot delete variant that has been ordered. The variant is in order history.");

            // ✅ Kiểm tra có trong giỏ hàng không
            var isUsedInCarts = await variantRepository.IsUsedInCartsAsync(id);
            if (isUsedInCarts) throw new PlantVariantException("Cannot delete variant that is in customer carts. Please wait or notify customers.");

            // ✅ Kiểm tra có inventory không
            var hasInventory = await variantRepository.HasInventoryAsync(id);
            if (hasInventory) throw new PlantVariantException("Cannot delete variant with existing inventory. Please delete inventory first.");

            await variantRepository.DeleteAsync(variant);
        }

        public async Task ToggleActiveAsync(Guid id)
        {
            var variant = await variantRepository.GetByIdAsync(id) ?? throw new PlantVariantException($"Plant variant with ID {id} not found");

            variant.IsActive = !variant.IsActive;
            variant.UpdatedAtUtc = DateTimeOffset.UtcNow;

            await variantRepository.UpdateAsync(variant);
        }

        private async Task<PlantVariantResponse> MapToResponseAsync(PlantVariant plantVariant)
        {
            return new PlantVariantResponse
            {
                Id = plantVariant.Id,
                PlantId = plantVariant.PlantId,
                SKU = plantVariant.SKU,
                Price = plantVariant.Price,
                Size = plantVariant.Size,
                IsActive = plantVariant.IsActive,
                CreatedAtUtc = plantVariant.CreatedAtUtc,
                UpdatedAtUtc = plantVariant.UpdatedAtUtc
            };
        }
    }
}