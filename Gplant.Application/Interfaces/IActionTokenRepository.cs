using Gplant.Domain.Entities;
using Gplant.Domain.enums;

namespace Gplant.Application.Interfaces
{
    public interface IActionTokenRepository
    {
        public Task<ActionToken?> CreateActionTokenAsync(Guid userId);
        public Task<ActionToken?> GetActionTokenAsync(Guid userId, ActionTokenPurpose actionTokenPurpose);
        public Task UpdateActionTokenAsync(ActionToken resetPasswordToken);
    }
}
