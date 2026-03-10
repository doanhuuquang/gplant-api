using Gplant.Domain.Entities;

namespace Gplant.Application.Interfaces
{
    public interface ILightningSaleRepository
    {
        Task<LightningSale?> GetByIdAsync(Guid id);
        Task<List<LightningSale>> GetAllAsync();
        Task<List<LightningSale>> GetActiveAsync();
        Task<List<LightningSale>> GetUpcomingAsync();
        Task<List<LightningSale>> GetOngoingAsync();
        Task<LightningSale?> GetCurrentActiveSaleAsync();
        Task CreateAsync(LightningSale sale);
        Task UpdateAsync(LightningSale sale);
        Task DeleteAsync(LightningSale sale);
    }
}