using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.Plant;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gplant.Infrastructure.Repositories
{
    public class PlantRepository(ApplicationDbContext context) : IPlantRepository
    {
        public async Task<Plant?> GetByIdAsync(Guid id)
        {
            return await context.Plants.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Plant?> GetBySlugAsync(string slug)
        {
            return await context.Plants.FirstOrDefaultAsync(p => p.Slug == slug);
        }

        public async Task<PagedResult<Plant>> GetPlantsAsync(PlantFilterRequest filter)
        {
            var query = context.Plants.AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(searchTerm) || 
                    p.Description.ToLower().Contains(searchTerm));
            }

            // Filter by category
            if (filter.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
            }

            // Filter by active status
            if (filter.IsActive.HasValue)
            {
                query = query.Where(p => p.IsActive == filter.IsActive.Value);
            }

            // Price range filter (requires join with variants)
            if (filter.MinPrice.HasValue || filter.MaxPrice.HasValue)
            {
                query = query.Where(p => context.PlantVariants
                    .Any(v => v.PlantId == p.Id && 
                        (!filter.MinPrice.HasValue || v.Price >= filter.MinPrice.Value) &&
                        (!filter.MaxPrice.HasValue || v.Price <= filter.MaxPrice.Value)));
            }

            // Sorting
            query = filter.SortBy?.ToLower() switch
            {
                "price" => filter.SortOrder?.ToLower() == "desc" 
                    ? query.OrderByDescending(p => context.PlantVariants.Where(v => v.PlantId == p.Id).Min(v => v.Price))
                    : query.OrderBy(p => context.PlantVariants.Where(v => v.PlantId == p.Id).Min(v => v.Price)),
                "createdat" => filter.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.CreatedAtUtc)
                    : query.OrderBy(p => p.CreatedAtUtc),
                _ => filter.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name)
            };

            var totalCount = await query.CountAsync();

            var items = await query.Skip((filter.PageNumber - 1) * filter.PageSize)
                                   .Take(filter.PageSize)
                                   .AsNoTracking()
                                   .ToListAsync();

            return new PagedResult<Plant>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<List<Plant>> GetPlantsByCategoryAsync(Guid categoryId)
        {
            return await context.Plants
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task CreateAsync(Plant plant)
        {
            await context.Plants.AddAsync(plant);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Plant plant)
        {
            context.Plants.Update(plant);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Plant plant)
        {
            context.Plants.Remove(plant);
            await context.SaveChangesAsync();
        }

        public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null)
        {
            return await context.Plants.AnyAsync(p => p.Slug == slug && (!excludeId.HasValue || p.Id != excludeId.Value));
        }

        public async Task<bool> HasVariantsAsync(Guid plantId)
        {
            return await context.PlantVariants.AnyAsync(v => v.PlantId == plantId);
        }

        public async Task<bool> IsUsedInOrdersAsync(Guid plantId)
        {
            // Kiểm tra qua PlantVariant → OrderItem
            var variantIds = await context.PlantVariants.Where(v => v.PlantId == plantId)
                                                        .Select(v => v.Id)
                                                        .ToListAsync();

            return await context.OrderItems.AnyAsync(oi => variantIds.Contains(oi.PlantVariantId));
        }
    }
}