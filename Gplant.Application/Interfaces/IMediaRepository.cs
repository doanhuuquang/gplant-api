using Gplant.Domain.Entities;

namespace Gplant.Application.Interfaces
{
    public interface IMediaRepository
    {
        Task<Media?> GetByIdAsync(Guid id);
        Task<Media?> GetByFileHashAsync(string fileHash);
        Task<List<Media>> GetAllAsync(int pageNumber = 1, int pageSize = 50, string? searchTerm = null);
        Task<int> GetTotalCountAsync(string? searchTerm = null);
        Task<Media> CreateAsync(Media media);
        Task DeleteAsync(Guid id);
        Task<bool> IsMediaInUseAsync(Guid mediaId);
        Task<bool> HasMediaByUserIdAsync(Guid userId);
    }
}