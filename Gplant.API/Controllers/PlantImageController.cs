using Gplant.API.ApiResponse;
using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.PlantImage;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gplant.API.Controllers
{
    [Route("api/plant-images")]
    [ApiController]
    public class PlantImageController(IPlantImageService imageService) : ControllerBase
    {
        /// <summary>
        /// Get image by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetImageById(Guid id)
        {
            var image = await imageService.GetByIdAsync(id);

            var response = new SuccessResponse<PlantImageResponse>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get plant image successful.",
                Data: image,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get images by plant ID
        /// </summary>
        [HttpGet("by-plant/{plantId:guid}")]
        public async Task<IActionResult> GetImagesByPlantId(Guid plantId)
        {
            var images = await imageService.GetByPlantIdAsync(plantId);

            var response = new SuccessResponse<List<PlantImageResponse>>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get plant images successful.",
                Data: images,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Create new image (Admin/Manager only)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> CreateImage(CreatePlantImageRequest request)
        {
            var image = await imageService.CreateAsync(request);

            var response = new SuccessResponse<PlantImageResponse>(
                StatusCode: StatusCodes.Status201Created,
                Message: "Create plant image successful.",
                Data: image,
                Timestamp: DateTime.UtcNow
            );

            return CreatedAtAction(nameof(GetImageById), new { id = image.Id }, response);
        }

        /// <summary>
        /// Delete image (Admin/Manager only)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> DeleteImage(Guid id)
        {
            await imageService.DeleteAsync(id);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Delete plant image successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Set image as primary (Admin/Manager only)
        /// </summary>
        [HttpPatch("{id:guid}/set-primary")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> SetPrimaryImage(Guid id)
        {
            await imageService.SetPrimaryImageAsync(id);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Primary image set successfully.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }
    }
}