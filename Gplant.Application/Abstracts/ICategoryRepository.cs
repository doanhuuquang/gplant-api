using Gplant.Domain.Entities;

namespace Gplant.Application.Abstracts
{
    public interface ICategoryRepository
    {
        public Task<List<Category>> GetCategoriesAsync();
        public Task<Category?> GetCategoryByIdAsync(Guid id);
        public Task<Category?> GetCategoryBySlugAsync(string slug);
        Task<List<Category>> GetCategoriesByParentId(Guid parentId);
        public Task CreateCategoryAsync(Category category);
        public Task UpdateCategoryAsync(Category category);
        public Task DeleteCategoryAsync(Category category);
    }
}
