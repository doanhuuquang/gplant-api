using Gplant.Domain.DTOs.Requests.CareInstruction;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;

namespace Gplant.Application.Interfaces
{
    public interface ICareInstructionService
    {
        Task<CareInstructionResponse> GetByIdAsync(Guid id);
        Task<List<CareInstructionResponse>> GetAllAsync();
        Task<CareInstructionResponse> CreateAsync(CreateCareInstructionRequest request);
        Task UpdateAsync(Guid id, UpdateCareInstructionRequest request);
        Task DeleteAsync(Guid id);
    }
}