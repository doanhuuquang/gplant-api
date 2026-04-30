using Gplant.Domain.DTOs.Requests.Folder;
using Gplant.Domain.DTOs.Responses;

namespace Gplant.Application.Interfaces
{
    public interface IFolderService
    {
        Task<FolderResponse> CreateAsync(CreateFolderRequest request);
        Task<FolderResponse> GetByIdAsync(Guid id);
        Task<FolderResponse> GetBySlugAsync(string slug);
        Task<List<FolderResponse>> GetAllAsync();
        Task DeleteAsync(Guid id);
        Task UpdateMediaCountAsync(Guid folderId, int increment);
        Task<FolderResponse> GetOrCreateUserUploadFolderAsync();
    }
}