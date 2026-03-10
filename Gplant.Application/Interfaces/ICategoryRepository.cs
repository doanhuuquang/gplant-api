using Gplant.Domain.Entities;

namespace Gplant.Application.Interfaces
{
    public interface ICategoryRepository
    {
        public Task<List<Category>> GetCategoriesAsync();
        public Task<List<Category>> GetActiveCategoriesAsync();
        public Task<Category?> GetCategoryByIdAsync(Guid id);
        public Task<Category?> GetCategoryBySlugAsync(string slug);
        Task<List<Category>> GetSubCategoriesByParentIdAsync(Guid? parentId);
        public Task CreateCategoryAsync(Category category);
        public Task UpdateCategoryAsync(Category category);
        public Task DeleteCategoryAsync(Category category);
        Task<bool> HasPlantsAsync(Guid categoryId);
    }
}
