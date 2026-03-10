using Gplant.Domain.DTOs.Requests.Category;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;

namespace Gplant.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryResponse>> GetCategoriesAsync();
        Task<List<CategoryResponse>> GetActiveCategoriesAsync();
        Task<CategoryResponse> GetCategoryBySlugAsync(string slug);
        Task<CategoryResponse> GetCategoryByIdAsync(Guid id);
        Task CreateCategoryAsync(CreateCategoryRequest createCategoryRequest);
        Task UpdateCategoryAsync(Guid id, UpdateCategoryRequest updateCategoryRequest);
        Task DeleteCategoryAsync(Guid id);
        Task ToggleActiveAsync(Guid id);
        Task<List<CategoryResponse>> GetSubCategoriesByParentIdAsync(Guid? parentId);  // ✅ NEW
    }
}
