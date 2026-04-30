using Gplant.Application.Interfaces;
using Gplant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gplant.Infrastructure.Repositories
{
    public class FolderRepository(ApplicationDbContext context) : IFolderRepository
    {
        public async Task<Folder> CreateAsync(Folder folder)
        {
            await context.Folders.AddAsync(folder);
            await context.SaveChangesAsync();
            return folder;
        }

        public async Task<Folder?> GetByIdAsync(Guid id)
        {
            return await context.Folders.FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Folder?> GetBySlugAsync(string slug)
        {
            return await context.Folders.FirstOrDefaultAsync(f => f.Slug == slug);
        }

        public async Task<List<Folder>> GetAllAsync()
        {
            return await context.Folders
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await context.Folders.CountAsync();
        }

        public async Task UpdateAsync(Folder folder)
        {
            context.Folders.Update(folder);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var folder = await context.Folders.FindAsync(id);
            if (folder != null)
            {
                context.Folders.Remove(folder);
                await context.SaveChangesAsync();
            }
        }
    }
}