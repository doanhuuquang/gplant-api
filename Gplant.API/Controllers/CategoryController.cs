using Gplant.API.ApiResponse;
using Gplant.Application.Abstracts;
using Gplant.Domain.DTOs.Requests;
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
                StatusCode: 200,
                Message: "Create category successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
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
                StatusCode: 200,
                Message: "Category status updated successfully.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await categoryService.GetCategoriesAsync();

            var response = new SuccessResponse<List<Category>>(
                StatusCode: 200,
                Message: "Get all categories successful.",
                Data: categories,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="createCategoryRequest"></param>
        /// <returns></returns>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> UpdateCategory(Guid id, UpdateCategoryRequest updateCategoryRequest)
        {
            await categoryService.UpdateCategoryAsync(id, updateCategoryRequest);

            var response = new SuccessResponse<object?>(
                StatusCode: 200,
                Message: "Update category successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            await categoryService.DeleteCategoryAsync(id);

            var response = new SuccessResponse<object?>(
                StatusCode: 200,
                Message: "Delete category successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }
    }
}
