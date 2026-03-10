using Gplant.API.ApiResponse;
using Gplant.Application.Interfaces;
using Gplant.Application.Services;
using Gplant.Domain.DTOs.Requests.Category;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gplant.API.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoryController(ICategoryService categoryService) : ControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="createCategoryRequest"></param>
        /// <returns></returns>
        [HttpPost("")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> CreateCategory(CreateCategoryRequest createCategoryRequest)
        {
            await categoryService.CreateCategoryAsync(createCategoryRequest);

            var response = new SuccessResponse<object?>(
                StatusCode  : StatusCodes.Status200OK,
                Message     : "Create category successful.",
                Data        : null,
                Timestamp   : DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPatch("{id:guid}/toggle-active")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            await categoryService.ToggleActiveAsync(id);

            var response = new SuccessResponse<object?>(
                StatusCode  : StatusCodes.Status200OK,
                Message     : "Category status updated successfully.",
                Data        : null,
                Timestamp   : DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await categoryService.GetCategoriesAsync();

            var response = new SuccessResponse<List<CategoryResponse>>(
                StatusCode  : StatusCodes.Status200OK,
                Message     : "Get all categories successful.",
                Data        : categories,
                Timestamp   : DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveCategories()
        {
            var categories = await categoryService.GetActiveCategoriesAsync();

            var response = new SuccessResponse<List<CategoryResponse>>(
                StatusCode  : StatusCodes.Status200OK,
                Message     : "Get active categories successful.",
                Data        : categories,
                Timestamp   : DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get category by slug
        /// </summary>
        [HttpGet("slug/{slug}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategoryBySlug(string slug)
        {
            var category = await categoryService.GetCategoryBySlugAsync(slug);

            var response = new SuccessResponse<CategoryResponse>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get category by slug successful.",
                Data: category,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        [HttpGet("id/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategoryBySlug(Guid id)
        {
            var category = await categoryService.GetCategoryByIdAsync(id);

            var response = new SuccessResponse<CategoryResponse>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get category by id successful.",
                Data: category,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateCategoryRequest"></param>
        /// <returns></returns>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> UpdateCategory(Guid id, UpdateCategoryRequest updateCategoryRequest)
        {
            await categoryService.UpdateCategoryAsync(id, updateCategoryRequest);

            var response = new SuccessResponse<object?>(
                StatusCode  : StatusCodes.Status200OK,
                Message     : "Update category successful.",
                Data        : null,
                Timestamp   : DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            await categoryService.DeleteCategoryAsync(id);

            var response = new SuccessResponse<object?>(
                StatusCode  : StatusCodes.Status200OK,
                Message     : "Delete category successful.",
                Data        : null,
                Timestamp   : DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get categories by parent ID (get children of a category)
        /// </summary>
        /// <param name="parentId">Parent category ID. Use null or "root" to get root categories.</param>
        [HttpGet("{parentId:guid?}/sub-category")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSubCategoriesByParentId(Guid? parentId)
        {
            var categories = await categoryService.GetSubCategoriesByParentIdAsync(parentId);

            var response = new SuccessResponse<List<CategoryResponse>>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get sub categories by parent successful.",
                Data: categories,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }
    }
}
