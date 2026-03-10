using Gplant.Domain.DTOs.Responses;
using Microsoft.AspNetCore.Http;

namespace Gplant.Application.Interfaces
{
    public interface IMediaService
    {
        Task<MediaResponse> UploadImageAsync(IFormFile file, Guid uploadedBy);
        Task<MediaResponse> GetByIdAsync(Guid id);
        Task<PagedResult<MediaResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 50, string? searchTerm = null);
        Task DeleteAsync(Guid id);
    }
}