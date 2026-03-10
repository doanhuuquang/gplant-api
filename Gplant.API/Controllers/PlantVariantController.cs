using Gplant.API.ApiResponse;
using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.PlantVariant;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gplant.API.Controllers
{
    [Route("api/plant-variants")]
    [ApiController]
    public class PlantVariantController(IPlantVariantService variantService) : ControllerBase
    {
        /// <summary>
        /// Get variant by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetVariantById(Guid id)
        {
            var variant = await variantService.GetByIdAsync(id);

            var response = new SuccessResponse<PlantVariantResponse>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get plant variant successful.",
                Data: variant,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get variants by plant ID
        /// </summary>
        [HttpGet("by-plant/{plantId:guid}")]
        public async Task<IActionResult> GetVariantsByPlantId(Guid plantId)
        {
            var variants = await variantService.GetByPlantIdAsync(plantId);

            var response = new SuccessResponse<List<PlantVariantResponse>>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get plant variants successful.",
                Data: variants,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Create new variant (Admin/Manager only)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> CreateVariant(CreatePlantVariantRequest request)
        {
            var variant = await variantService.CreateAsync(request);

            var response = new SuccessResponse<PlantVariantResponse>(
                StatusCode: StatusCodes.Status201Created,
                Message: "Create plant variant successful.",
                Data: variant,
                Timestamp: DateTime.UtcNow
            );

            return CreatedAtAction(nameof(GetVariantById), new { id = variant.Id }, response);
        }

        /// <summary>
        /// Update variant (Admin/Manager only)
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> UpdateVariant(Guid id, UpdatePlantVariantRequest request)
        {
            await variantService.UpdateAsync(id, request);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Update plant variant successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Delete variant (Admin/Manager only)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> DeleteVariant(Guid id)
        {
            await variantService.DeleteAsync(id);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Delete plant variant successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Toggle variant active status (Admin/Manager only)
        /// </summary>
        [HttpPatch("{id:guid}/toggle-active")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            await variantService.ToggleActiveAsync(id);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Plant variant status updated successfully.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }
    }
}