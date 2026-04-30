using Gplant.API.ApiResponse;
using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gplant.API.Controllers
{
    [Route("api/medias")]
    [ApiController]
    public class MediaController(IMediaService mediaService, IFolderService folderService) : ControllerBase
    {
        /// <summary>
        /// Upload image to folder
        /// </summary>
        [HttpPost("upload")]
        [Authorize]
        public async Task<ActionResult<MediaResponse>> UploadImage(
            IFormFile file,
            Guid? folderId)
        {
            var userId = GetCurrentUserId();
            
            // Nếu không chỉ định folder, dùng folder mặc định
            var actualFolderId = folderId ?? (await folderService.GetOrCreateUserUploadFolderAsync()).Id;
            
            var media = await mediaService.UploadImageAsync(file, userId, actualFolderId);

            var response = new SuccessResponse<MediaResponse>(
                StatusCode: StatusCodes.Status201Created,
                Message: "Upload image successful.",
                Data: media,
                Timestamp: DateTime.UtcNow
            );

            return CreatedAtAction(nameof(GetMediaById), new { id = media.Id }, response);
        }

        /// <summary>
        /// Get media by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<MediaResponse>> GetMediaById(Guid id)
        {
            var media = await mediaService.GetByIdAsync(id);

            var response = new SuccessResponse<MediaResponse>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get media successful.",
                Data: media,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get all media (paginated with search)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<MediaResponse>>> GetAllMedia(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 50,
            [FromQuery] string? searchTerm = null)
        {
            var result = await mediaService.GetAllAsync(pageNumber, pageSize, searchTerm);

            var response = new SuccessResponse<PagedResult<MediaResponse>>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get all media successful.",
                Data: result,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get media by folder ID (paginated)
        /// </summary>
        [HttpGet("folder/{folderId:guid}")]
        public async Task<ActionResult<PagedResult<MediaResponse>>> GetMediaByFolder(
            Guid folderId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            var result = await mediaService.GetByFolderIdAsync(folderId, pageNumber, pageSize);

            var response = new SuccessResponse<PagedResult<MediaResponse>>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get media by folder successful.",
                Data: result,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Delete media
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<ActionResult<object?>> DeleteMedia(Guid id)
        {
            await mediaService.DeleteAsync(id);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Delete media successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("User not authenticated");

            return userId;
        }
    }
}