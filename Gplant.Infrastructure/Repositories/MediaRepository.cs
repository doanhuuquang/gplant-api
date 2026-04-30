using Gplant.Application.Interfaces;
using Gplant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gplant.Infrastructure.Repositories
{
    public class MediaRepository(ApplicationDbContext context) : IMediaRepository
    {
        public async Task<Media?> GetByIdAsync(Guid id)
        {
            return await context.Medias
                .Where(m => !m.IsDeleted)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Media?> GetByFileHashAsync(string fileHash)
        {
            return await context.Medias
                .Where(m => !m.IsDeleted)
                .FirstOrDefaultAsync(m => m.FileHash == fileHash);
        }

        public async Task<List<Media>> GetAllAsync(int pageNumber = 1, int pageSize = 50, string? searchTerm = null)
        {
            var query = context.Medias.Where(m => !m.IsDeleted);

            // Tìm kiếm theo FileName hoặc MimeType
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(m =>
                    m.FileName.Contains(searchTerm) ||
                    m.MimeType.Contains(searchTerm));
            }

            return await query
                .OrderByDescending(m => m.CreatedAtUtc)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(string? searchTerm = null)
        {
            var query = context.Medias.Where(m => !m.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(m =>
                    m.FileName.Contains(searchTerm) ||
                    m.MimeType.Contains(searchTerm));
            }

            return await query.CountAsync();
        }

        public async Task<int> GetTotalCountByFolderAsync(Guid folderId)
        {
            return await context.Medias
                .Where(m => m.FolderId == folderId && !m.IsDeleted)
                .CountAsync();
        }

        public async Task<Media> CreateAsync(Media media)
        {
            await context.Medias.AddAsync(media);
            await context.SaveChangesAsync();
            return media;
        }

        public async Task DeleteAsync(Guid id)
        {
            var media = await context.Medias.FindAsync(id);
            if (media != null)
            {
                media.IsDeleted = true;
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsMediaInUseAsync(Guid mediaId)
        {
            if (await context.Categories.AnyAsync(c => c.MediaId == mediaId))
                return true;

            if (await context.PlantImages.AnyAsync(pi => pi.MediaId == mediaId))
                return true;

            if (await context.Banners.AnyAsync(b => b.MediaId == mediaId))
                return true;

            return false;
        }

        public async Task<bool> HasMediaByUserIdAsync(Guid userId)
        {
            return await context.Medias.AnyAsync(m => m.UploadedBy == userId && !m.IsDeleted);
        }

        public async Task<List<Media>> GetByFolderIdAsync(Guid folderId, int pageNumber = 1, int pageSize = 50)
        {
            var query = context.Medias
                .Where(m => m.FolderId == folderId && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedAtUtc)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return await query.ToListAsync();
        }
    }
}