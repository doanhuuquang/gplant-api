using Gplant.Domain.DTOs.Responses;
using Microsoft.AspNetCore.Http;

namespace Gplant.Application.Interfaces
{
    public interface IMediaService
    {
        Task<MediaResponse> UploadImageAsync(IFormFile file, Guid uploadedBy, Guid folderId);
        Task<MediaResponse> GetByIdAsync(Guid id);
        Task<PagedResult<MediaResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 50, string? searchTerm = null);
        Task<PagedResult<MediaResponse>> GetByFolderIdAsync(Guid folderId, int pageNumber = 1, int pageSize = 50);
        Task DeleteAsync(Guid id);
    }
}