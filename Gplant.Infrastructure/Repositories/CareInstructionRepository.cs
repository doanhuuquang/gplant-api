using Gplant.Application.Interfaces;
using Gplant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gplant.Infrastructure.Repositories
{
    public class CareInstructionRepository(ApplicationDbContext context) : ICareInstructionRepository
    {
        public async Task<CareInstruction?> GetByIdAsync(Guid id)
        {
            return await context.CareInstructions.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<CareInstruction>> GetAllAsync()
        {
            return await context.CareInstructions.AsNoTracking().ToListAsync();
        }

        public async Task CreateAsync(CareInstruction careInstruction)
        {
            await context.CareInstructions.AddAsync(careInstruction);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CareInstruction careInstruction)
        {
            context.CareInstructions.Update(careInstruction);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(CareInstruction careInstruction)
        {
            context.CareInstructions.Remove(careInstruction);
            await context.SaveChangesAsync();
        }

        public async Task<bool> IsUsedByPlantsAsync(Guid careInstructionId)
        {
            return await context.Plants.AnyAsync(p => p.CareInstructionId == careInstructionId);
        }
    }
}