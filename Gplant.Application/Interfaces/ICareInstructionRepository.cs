using Gplant.Domain.Entities;

namespace Gplant.Application.Interfaces
{
    public interface ICareInstructionRepository
    {
        Task<CareInstruction?> GetByIdAsync(Guid id);
        Task<List<CareInstruction>> GetAllAsync();
        Task CreateAsync(CareInstruction careInstruction);
        Task UpdateAsync(CareInstruction careInstruction);
        Task DeleteAsync(CareInstruction careInstruction);
        Task<bool> IsUsedByPlantsAsync(Guid careInstructionId);
    }
}