using Gplant.API.ApiResponse;
using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.Plant;
using Gplant.Domain.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gplant.API.Controllers
{
    [Route("api/plants")]
    [ApiController]
    public class PlantController(IPlantService plantService) : ControllerBase
    {
        /// <summary>
        /// Get all plants with filters and pagination (Public)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPlants([FromQuery] PlantFilterRequest filter)
        {
            var plants = await plantService.GetPlantsAsync(filter);

            var response = new SuccessResponse<PagedResult<PlantResponse>>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get plants successful.",
                Data: plants,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get plant by ID (Public)
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPlantById(Guid id)
        {
            var plant = await plantService.GetByIdAsync(id);

            var response = new SuccessResponse<PlantResponse>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get plant successful.",
                Data: plant,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get plant by slug (Public)
        /// </summary>
        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetPlantBySlug(string slug)
        {
            var plant = await plantService.GetBySlugAsync(slug);

            var response = new SuccessResponse<PlantResponse>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get plant successful.",
                Data: plant,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get plants by category (Public)
        /// </summary>
        [HttpGet("by-category/{categoryId:guid}")]
        public async Task<IActionResult> GetPlantsByCategory(Guid categoryId)
        {
            var plants = await plantService.GetPlantsByCategoryAsync(categoryId);

            var response = new SuccessResponse<List<PlantResponse>>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get plants by category successful.",
                Data: plants,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Create new plant (Admin/Manager only)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> CreatePlant(CreatePlantRequest request)
        {
            var plant = await plantService.CreateAsync(request);

            var response = new SuccessResponse<PlantResponse>(
                StatusCode: StatusCodes.Status201Created,
                Message: "Create plant successful.",
                Data: plant,
                Timestamp: DateTime.UtcNow
            );

            return CreatedAtAction(nameof(GetPlantById), new { id = plant.Id }, response);
        }

        /// <summary>
        /// Update plant (Admin/Manager only)
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> UpdatePlant(Guid id, UpdatePlantRequest request)
        {
            await plantService.UpdateAsync(id, request);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Update plant successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Delete plant (Admin/Manager only)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> DeletePlant(Guid id)
        {
            await plantService.DeleteAsync(id);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Delete plant successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Toggle plant active status (Admin/Manager only)
        /// </summary>
        [HttpPatch("{id:guid}/toggle-active")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            await plantService.ToggleActiveAsync(id);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Plant status updated successfully.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }
    }
}