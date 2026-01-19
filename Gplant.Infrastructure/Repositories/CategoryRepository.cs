using Gplant.Application.Abstracts;
using Gplant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gplant.Infrastructure.Repositories
{
    public class CategoryRepository(ApplicationDbContext applicationDbContext) : ICategoryRepository
    {
        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await applicationDbContext.Categories.ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(Guid id)
        {
            return await applicationDbContext.Categories.FindAsync(id);
        }

        public async Task<Category?> GetCategoryBySlugAsync(string slug)
        {
            return await applicationDbContext.Categories.FirstOrDefaultAsync(category => category.Slug == slug);
        }

        public async Task<List<Category>> GetCategoriesByParentId(Guid parentId)
        {
            return await applicationDbContext.Categories.Where(c => c.ParentId == parentId).ToListAsync();
        }

        public async Task CreateCategoryAsync(Category category)
        {
            await applicationDbContext.Categories.AddAsync(category);
            await applicationDbContext.SaveChangesAsync();
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            await applicationDbContext.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(Category category)
        {
            await applicationDbContext.Categories.Where(c => c.Id == category.Id).ExecuteDeleteAsync();
        }
    }
}
