using Gplant.Application.Abstracts;
using Gplant.Application.Helpers;
using Gplant.Domain.DTOs.Requests;
using Gplant.Domain.Entities;
using Gplant.Domain.Exceptions;

namespace Gplant.Application.Services
{
    public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
    {
        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await categoryRepository.GetCategoriesAsync();
        }

        public async Task CreateCategoryAsync(CreateCategoryRequest createCategoryRequest)
        {
            if (string.IsNullOrWhiteSpace(createCategoryRequest.Name)) throw new CategoryException("Category name is required");
            if (string.IsNullOrWhiteSpace(createCategoryRequest.Description)) throw new CategoryException("Description name is required");
            if (string.IsNullOrWhiteSpace(createCategoryRequest.ImageUrl)) throw new CategoryException("ImageUrl name is required");

            Category? parentCategory = null;

            if (createCategoryRequest.ParentId.HasValue)
            {
                parentCategory = await categoryRepository.GetCategoryByIdAsync(createCategoryRequest.ParentId.Value) ?? throw new CategoryException("Parent category does not exist");
            }

            var slug = SlugHelper.GenerateSlug(createCategoryRequest.Name);

            var existingCategory = await categoryRepository.GetCategoryBySlugAsync(slug);

            if (existingCategory != null) throw new CategoryException("Category with the same name already exists");

            var category = new Category
            {
                Id          = Guid.NewGuid(),
                Name        = createCategoryRequest.Name,
                Description = createCategoryRequest.Description,
                ImageUrl    = createCategoryRequest.ImageUrl,
                Slug        = slug,
                ParentId    = parentCategory?.Id,
            };

            await categoryRepository.CreateCategoryAsync(category);
        }

        public async Task UpdateCategoryAsync(Guid id, UpdateCategoryRequest updateCategoryRequest)
        {
            if (string.IsNullOrWhiteSpace(updateCategoryRequest.Name)) throw new CategoryException("Category name is required");
            if (string.IsNullOrWhiteSpace(updateCategoryRequest.Description)) throw new CategoryException("Description name is required");
            if (string.IsNullOrWhiteSpace(updateCategoryRequest.ImageUrl)) throw new CategoryException("ImageUrl name is required");

            var slug = SlugHelper.GenerateSlug(updateCategoryRequest.Name);

            Category? existingCategory = await categoryRepository.GetCategoryByIdAsync(id) ?? throw new CategoryException("Category does not exist");

            Category? parentCategory = null;
            if (updateCategoryRequest.ParentId.HasValue)
            {
                parentCategory = await categoryRepository.GetCategoryByIdAsync(updateCategoryRequest.ParentId.Value) ?? throw new CategoryException("Parent category does not exist");
            }

            if (existingCategory.Id == updateCategoryRequest.ParentId) throw new CategoryException("A category cannot be its own parent.");

            var existingWithSameSlug = await categoryRepository.GetCategoryBySlugAsync(slug);
            if (existingWithSameSlug != null && existingWithSameSlug.Id != existingCategory.Id) throw new CategoryException("Category with the same name already exists");

            existingCategory.Name           = updateCategoryRequest.Name;
            existingCategory.Description    = updateCategoryRequest.Description;
            existingCategory.ImageUrl       = updateCategoryRequest.ImageUrl;
            existingCategory.Slug           = slug;
            existingCategory.ParentId       = parentCategory?.Id;
            existingCategory.IsActive       = updateCategoryRequest.IsActive;
            existingCategory.UpdatedAtUtc   = DateTime.UtcNow;

            await categoryRepository.UpdateCategoryAsync(existingCategory);
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            var existingCategory = await categoryRepository.GetCategoryByIdAsync(id) 
                                ?? throw new CategoryException("Category does not exist");

            var childCategories = await categoryRepository.GetCategoriesByParentId(existingCategory.Id);
            if (childCategories.Count > 0) 
                throw new CategoryException("Cannot delete category with existing subcategories. Please delete or reassign its subcategories first.");

            await categoryRepository.DeleteCategoryAsync(existingCategory);
        }

        public async Task ToggleActiveAsync(Guid id)
        {
            var category = await categoryRepository.GetCategoryByIdAsync(id) ?? throw new CategoryException("Category not found.");

            category.IsActive       = !category.IsActive;
            category.UpdatedAtUtc   = DateTime.UtcNow;

            await categoryRepository.UpdateCategoryAsync(category);
        }
    }
}
