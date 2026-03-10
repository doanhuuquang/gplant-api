using Gplant.Application.Helpers;
using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.Category;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;
using Gplant.Domain.Exceptions.Category;
using Gplant.Domain.Exceptions.Media;

namespace Gplant.Application.Services
{
    public class CategoryService(ICategoryRepository categoryRepository, IMediaRepository mediaRepository) : ICategoryService
    {
        public async Task<List<CategoryResponse>> GetCategoriesAsync()
        {
            var categories = await categoryRepository.GetCategoriesAsync();
            
            var responses = new List<CategoryResponse>();
            foreach (var category in categories)
            {
                responses.Add(await MapToResponseAsync(category));
            }
            
            return responses;
        }

        public async Task<List<CategoryResponse>> GetActiveCategoriesAsync()
        {
            var categories = await categoryRepository.GetActiveCategoriesAsync();
            
            var responses = new List<CategoryResponse>();
            foreach (var category in categories)
            {
                responses.Add(await MapToResponseAsync(category));
            }
            
            return responses;
        }

        public async Task<CategoryResponse> GetCategoryByIdAsync(Guid id)
        {
            var category = await categoryRepository.GetCategoryByIdAsync(id)
                ?? throw new CategoryNotFoundException($"Category with ID {id} not found");

            return await MapToResponseAsync(category);
        }

        public async Task<CategoryResponse> GetCategoryBySlugAsync(string slug)
        {
            var category = await categoryRepository.GetCategoryBySlugAsync(slug)
                ?? throw new CategoryNotFoundException($"Category with slug '{slug}' not found");

            return await MapToResponseAsync(category);
        }

        public async Task<List<CategoryResponse>> GetSubCategoriesByParentIdAsync(Guid? parentId)
        {
            var categories = await categoryRepository.GetSubCategoriesByParentIdAsync(parentId);
            
            var responses = new List<CategoryResponse>();
            foreach (var category in categories)
            {
                responses.Add(await MapToResponseAsync(category));
            }
            
            return responses;
        }

        public async Task CreateCategoryAsync(CreateCategoryRequest request)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new CategoryException("Category name is required");
            if (string.IsNullOrWhiteSpace(request.Description))
                throw new CategoryException("Description is required");

            // Validate media exists
            if (request.MediaId.HasValue)
                _ = await mediaRepository.GetByIdAsync(request.MediaId.Value) ?? throw new MediaNotFoundException("Media not found");

            // Validate parent category
            if (request.ParentId.HasValue)
                _ = await categoryRepository.GetCategoryByIdAsync(request.ParentId.Value) ?? throw new CategoryNotFoundException("Parent category does not exist");

            // Generate slug
            var slug = SlugHelper.GenerateSlug(request.Name);

            // Check slug uniqueness
            var existingCategory = await categoryRepository.GetCategoryBySlugAsync(slug);
            if (existingCategory != null)
                throw new CategoryException("Category with the same name already exists");

            // Create category
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                MediaId = request.MediaId,
                Slug = slug,
                ParentId = request.ParentId,
                IsActive = true,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            };

            await categoryRepository.CreateCategoryAsync(category);
        }

        public async Task UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new CategoryException("Category name is required");
            if (string.IsNullOrWhiteSpace(request.Description))
                throw new CategoryException("Description is required");

            // Get existing category
            var category = await categoryRepository.GetCategoryByIdAsync(id)
                ?? throw new CategoryNotFoundException($"Category with ID {id} not found");

            // Validate media exists
            if (request.MediaId.HasValue)
                _ = await mediaRepository.GetByIdAsync(request.MediaId.Value) ?? throw new MediaNotFoundException("Media not found");

            // Validate parent category
            if (request.ParentId.HasValue)
            {
                // Prevent self-reference
                if (id == request.ParentId.Value) throw new CategoryException("A category cannot be its own parent");
                _ = await categoryRepository.GetCategoryByIdAsync(request.ParentId.Value) ?? throw new CategoryNotFoundException("Parent category does not exist");

                // Check circular reference
                var hasCircularReference = await CheckCircularReferenceAsync(id, request.ParentId.Value);
                if (hasCircularReference)
                    throw new CategoryException("Cannot set parent: This would create a circular reference");
            }

            // Generate slug
            var slug = SlugHelper.GenerateSlug(request.Name);

            // Check slug uniqueness
            var existingWithSameSlug = await categoryRepository.GetCategoryBySlugAsync(slug);
            if (existingWithSameSlug != null && existingWithSameSlug.Id != id)
                throw new CategoryException("Category with the same name already exists");

            // Update category
            category.Name = request.Name;
            category.Description = request.Description;
            category.MediaId = request.MediaId;
            category.Slug = slug;
            category.ParentId = request.ParentId;
            category.IsActive = request.IsActive;
            category.UpdatedAtUtc = DateTimeOffset.UtcNow;

            await categoryRepository.UpdateCategoryAsync(category);
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            var category = await categoryRepository.GetCategoryByIdAsync(id) ?? throw new CategoryNotFoundException($"Category with ID {id} not found");

            // Check for child categories
            var childCategories = await categoryRepository.GetSubCategoriesByParentIdAsync(id);
            if (childCategories.Count > 0) throw new CategoryException("Cannot delete category with existing subcategories. Please delete or reassign them first.");

            var hasPlants = await categoryRepository.HasPlantsAsync(id);
            if (hasPlants) throw new CategoryException("Cannot delete category that is being used by products. Please reassign or delete the products first.");


            await categoryRepository.DeleteCategoryAsync(category);
        }

        public async Task ToggleActiveAsync(Guid id)
        {
            var category = await categoryRepository.GetCategoryByIdAsync(id)
                ?? throw new CategoryNotFoundException($"Category with ID {id} not found");

            category.IsActive = !category.IsActive;
            category.UpdatedAtUtc = DateTimeOffset.UtcNow;

            await categoryRepository.UpdateCategoryAsync(category);
        }

        private async Task<CategoryResponse> MapToResponseAsync(Category category)
        {
            Media? media = null;

            if (category.MediaId.HasValue)
            {
                media = await mediaRepository.GetByIdAsync(category.MediaId.Value);
            }

            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Description = category.Description,
                Media = media,
                ParentId = category.ParentId,
                IsActive = category.IsActive,
                CreatedAtUtc = category.CreatedAtUtc,
                UpdatedAtUtc = category.UpdatedAtUtc
            };
        }

        private async Task<bool> CheckCircularReferenceAsync(Guid categoryId, Guid newParentId)
        {
            var currentId = newParentId;
            var visited = new HashSet<Guid> { categoryId };
            var maxIterations = 100;
            var iterations = 0;

            while (currentId != Guid.Empty && iterations < maxIterations)
            {
                if (visited.Contains(currentId))
                {
                    return true;  // Circular reference detected
                }

                visited.Add(currentId);

                var current = await categoryRepository.GetCategoryByIdAsync(currentId);

                if (current?.ParentId == null)
                {
                    break;  // Reached root
                }

                currentId = current.ParentId.Value;
                iterations++;
            }

            return false;
        }
    }
}
