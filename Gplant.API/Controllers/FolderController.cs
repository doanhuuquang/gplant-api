using Gplant.API.ApiResponse;
using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.Folder;
using Gplant.Domain.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gplant.API.Controllers
{
    [Route("api/folders")]
    [ApiController]
    public class FolderController(IFolderService folderService) : ControllerBase
    {
        /// <summary>
        /// Create a new folder (Admin/Manager only)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<FolderResponse>> CreateFolder([FromBody] CreateFolderRequest request)
        {
            var folder = await folderService.CreateAsync(request);

            var response = new SuccessResponse<FolderResponse>(
                StatusCode: StatusCodes.Status201Created,
                Message: "Folder created successfully.",
                Data: folder,
                Timestamp: DateTime.UtcNow
            );

            return CreatedAtAction(nameof(GetFolderById), new { id = folder.Id }, response);
        }

        /// <summary>
        /// Get folder by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<FolderResponse>> GetFolderById(Guid id)
        {
            var folder = await folderService.GetByIdAsync(id);

            var response = new SuccessResponse<FolderResponse>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get folder successful.",
                Data: folder,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get folder by slug
        /// </summary>
        [HttpGet("by-slug/{slug}")]
        public async Task<ActionResult<FolderResponse>> GetFolderBySlug(string slug)
        {
            var folder = await folderService.GetBySlugAsync(slug);

            var response = new SuccessResponse<FolderResponse>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get folder by slug successful.",
                Data: folder,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get all folders
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<FolderResponse>>> GetAllFolders()
        {
            var result = await folderService.GetAllAsync();

            var response = new SuccessResponse<List<FolderResponse>>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get all folders successful.",
                Data: result,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Delete folder (Admin/Manager only)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOrManager")]
        public async Task<ActionResult<object?>> DeleteFolder(Guid id)
        {
            await folderService.DeleteAsync(id);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Folder deleted successfully.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }
    }
}