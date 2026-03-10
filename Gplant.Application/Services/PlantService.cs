using Gplant.Application.Helpers;
using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.Plant;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;
using Gplant.Domain.Exceptions.Plant;

namespace Gplant.Application.Services
{
    public class PlantService(
        IPlantRepository plantRepository,
        ICategoryRepository categoryRepository,
        ICategoryService categoryService,
        ICareInstructionService careInstructionService,
        IPlantVariantService plantVariantService,
        IPlantImageService plantImageService,
        ICareInstructionRepository careInstructionRepository) : IPlantService
    {
        public async Task<PlantResponse> GetByIdAsync(Guid id)
        {
            var plant = await plantRepository.GetByIdAsync(id) ?? throw new PlantNotFoundException($"Plant with ID {id} not found");
            return await MapToResponseAsync(plant);
        }

        public async Task<PlantResponse> GetBySlugAsync(string slug)
        {
            var plant = await plantRepository.GetBySlugAsync(slug) ?? throw new PlantNotFoundException($"Plant with slug '{slug}' not found");
            return await MapToResponseAsync(plant);
        }

        public async Task<PagedResult<PlantResponse>> GetPlantsAsync(PlantFilterRequest filter)
        {
            var pagedPlants = await plantRepository.GetPlantsAsync(filter);

            var plantResponses = new List<PlantResponse>();
            foreach (var plant in pagedPlants.Items)
            {
                plantResponses.Add(await MapToResponseAsync(plant));
            }

            return new PagedResult<PlantResponse>
            {
                Items = plantResponses,
                TotalCount = pagedPlants.TotalCount,
                PageNumber = pagedPlants.PageNumber,
                PageSize = pagedPlants.PageSize
            };
        }

        public async Task<List<PlantResponse>> GetPlantsByCategoryAsync(Guid categoryId)
        {
            _ = await categoryRepository.GetCategoryByIdAsync(categoryId) ?? throw new PlantException($"Category with ID {categoryId} not found");

            var plants = await plantRepository.GetPlantsByCategoryAsync(categoryId);

            var responses = new List<PlantResponse>();
            foreach (var plant in plants)
            {
                responses.Add(await MapToResponseAsync(plant));
            }

            return responses;
        }

        public async Task<PlantResponse> CreateAsync(CreatePlantRequest request)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(request.Name)) throw new PlantException("Plant name is required");
            if (string.IsNullOrWhiteSpace(request.ShortDescription)) throw new PlantException("Short description is required");
            if (string.IsNullOrWhiteSpace(request.Description)) throw new PlantException("Description is required");

            // Check category exists
            _ = await categoryRepository.GetCategoryByIdAsync(request.CategoryId) ?? throw new PlantException($"Category with ID {request.CategoryId} not found");

            // Check care instruction exists
            _ = await careInstructionRepository.GetByIdAsync(request.CareInstructionId) ?? throw new PlantException($"Care instruction with ID {request.CareInstructionId} not found");

            // Generate unique slug
            var slug = SlugHelper.GenerateSlug(request.Name);
            if (await plantRepository.SlugExistsAsync(slug))
            {
                slug = $"{slug}-{Guid.NewGuid().ToString()[..8]}";
            }

            var plant = new Plant
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Slug = slug,
                ShortDescription = request.ShortDescription,
                Description = request.Description,
                CategoryId = request.CategoryId,
                CareInstructionId = request.CareInstructionId,
                IsActive = request.IsActive,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            };

            await plantRepository.CreateAsync(plant);

            return await MapToResponseAsync(plant);
        }

        public async Task UpdateAsync(Guid id, UpdatePlantRequest request)
        {
            var plant = await plantRepository.GetByIdAsync(id) ?? throw new PlantNotFoundException($"Plant with ID {id} not found");

            if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != plant.Name)
            {
                plant.Name = request.Name;
                var newSlug = SlugHelper.GenerateSlug(request.Name);
                if (await plantRepository.SlugExistsAsync(newSlug, plant.Id))
                {
                    newSlug = $"{newSlug}-{Guid.NewGuid().ToString()[..8]}";
                }
                plant.Slug = newSlug;
            }

            if (!string.IsNullOrWhiteSpace(request.ShortDescription)) plant.ShortDescription = request.ShortDescription;

            if (!string.IsNullOrWhiteSpace(request.Description)) plant.Description = request.Description;

            if (request.CategoryId.HasValue && request.CategoryId.Value != plant.CategoryId)
            {
                _ = await categoryRepository.GetCategoryByIdAsync(request.CategoryId.Value) ?? throw new PlantException($"Category with ID {request.CategoryId} not found");
                plant.CategoryId = request.CategoryId.Value;
            }

            if (request.CareInstructionId.HasValue && request.CareInstructionId.Value != plant.CareInstructionId)
            {
                _ = await careInstructionRepository.GetByIdAsync(request.CareInstructionId.Value) ?? throw new PlantException($"Care instruction with ID {request.CareInstructionId} not found");
                plant.CareInstructionId = request.CareInstructionId.Value;
            }

            if (request.IsActive.HasValue) plant.IsActive = request.IsActive.Value;

            plant.UpdatedAtUtc = DateTimeOffset.UtcNow;

            await plantRepository.UpdateAsync(plant);
        }

        public async Task DeleteAsync(Guid id)
        {
            var plant = await plantRepository.GetByIdAsync(id) 
                ?? throw new PlantNotFoundException($"Plant with ID {id} not found");

            // ✅ Kiểm tra có variants không
            var hasVariants = await plantRepository.HasVariantsAsync(id);
            if (hasVariants) throw new PlantException("Cannot delete plant with existing variants. Please delete the variants first.");

            // ✅ Kiểm tra có trong đơn hàng không
            var isUsedInOrders = await plantRepository.IsUsedInOrdersAsync(id);
            if (isUsedInOrders) throw new PlantException("Cannot delete plant that has been ordered. The product is in order history.");

            // Cascade delete sẽ xử lý PlantImages
            await plantRepository.DeleteAsync(plant);
        }

        public async Task ToggleActiveAsync(Guid id)
        {
            var plant = await plantRepository.GetByIdAsync(id) ?? throw new PlantNotFoundException($"Plant with ID {id} not found");

            plant.IsActive = !plant.IsActive;
            plant.UpdatedAtUtc = DateTime.UtcNow;

            await plantRepository.UpdateAsync(plant);
        }

        private async Task<PlantResponse> MapToResponseAsync(Plant plant)
        {
            var variants = await plantVariantService.GetByPlantIdAsync(plant.Id);
            var images = await plantImageService.GetByPlantIdAsync(plant.Id);
            var category = await categoryService.GetCategoryByIdAsync(plant.CategoryId);
            var careInstruction = await careInstructionService.GetByIdAsync(plant.CareInstructionId);

            var activeVariants = variants.Where(v => v.IsActive).ToList();

            return new PlantResponse
            {
                Id = plant.Id,
                Name = plant.Name,
                Slug = plant.Slug,
                ShortDescription = plant.ShortDescription,
                Description = plant.Description,
                Category = category,
                CareInstruction = careInstruction,
                Variants = variants,
                Images = images,
                MinPrice = activeVariants.Count != 0 ? activeVariants.Min(v => v.Price) : null,
                MaxPrice = activeVariants.Count != 0 ? activeVariants.Max(v => v.Price) : null,
                IsActive = plant.IsActive,
                CreatedAtUtc = plant.CreatedAtUtc,
                UpdatedAtUtc = plant.UpdatedAtUtc
            };
        }
    }
}