using Gplant.Domain.Entities;

namespace Gplant.Application.Interfaces
{
    public interface IFolderRepository
    {
        Task<Folder> CreateAsync(Folder folder);
        Task<Folder?> GetByIdAsync(Guid id);
        Task<Folder?> GetBySlugAsync(string slug);
        Task<List<Folder>> GetAllAsync();  
        Task<int> GetTotalCountAsync();
        Task UpdateAsync(Folder folder);
        Task DeleteAsync(Guid id);
    }
}