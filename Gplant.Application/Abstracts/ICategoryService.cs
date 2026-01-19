using Gplant.Domain.DTOs.Requests;
using Gplant.Domain.Entities;

namespace Gplant.Application.Abstracts
{
    public interface ICategoryService
    {
        Task<List<Category>> GetCategoriesAsync();
        Task CreateCategoryAsync(CreateCategoryRequest createCategoryRequest);
        Task UpdateCategoryAsync(Guid id, UpdateCategoryRequest updateCategoryRequest);
        Task DeleteCategoryAsync(Guid id);
        Task ToggleActiveAsync(Guid id);
    }
}
