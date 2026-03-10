using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gplant.Infrastructure.Repositories
{
    public class PlantImageRepository(ApplicationDbContext context) : IPlantImageRepository
    {
        public async Task<PlantImage?> GetByIdAsync(Guid id)
        {
            return await context.PlantImages.FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<List<PlantImage>> GetByPlantIdAsync(Guid plantId)
        {
            var plantImages = await context.PlantImages
                                    .Where(i => i.PlantId == plantId)
                                    .OrderByDescending(i => i.IsPrimary)
                                    .ThenBy(i => i.CreatedAtUtc)
                                    .AsNoTracking()
                                    .ToListAsync();

            return plantImages;
        }

        public async Task<PlantImage?> GetPrimaryImageByPlantIdAsync(Guid plantId)
        {
            return await context.PlantImages
                .FirstOrDefaultAsync(i => i.PlantId == plantId && i.IsPrimary);
        }

        public async Task CreateAsync(PlantImage image)
        {
            await context.PlantImages.AddAsync(image);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PlantImage image)
        {
            context.PlantImages.Update(image);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(PlantImage image)
        {
            context.PlantImages.Remove(image);
            await context.SaveChangesAsync();
        }
    }
}