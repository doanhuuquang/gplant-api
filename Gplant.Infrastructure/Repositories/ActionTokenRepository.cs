using Microsoft.EntityFrameworkCore;
using Gplant.Application.Abstracts;
using Gplant.Domain.Entities;
using Gplant.Domain.enums;
using System.Security.Cryptography;

namespace Gplant.Infrastructure.Repositories
{
    public class ActionTokenRepository(ApplicationDbContext applicationDbContext) : IActionTokenRepository
    {
        public async Task<ActionToken?> CreateActionTokenAsync(Guid userId)
        {
            var randomNumber = new byte[64];
            RandomNumberGenerator.Create().GetBytes(randomNumber);
            var token = Convert.ToBase64String(randomNumber);

            var resetPasswordToken = new ActionToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = token,
                Purpose = ActionTokenPurpose.ResetPassword,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15),
                IsUsed = false
            };

            await applicationDbContext.ActionTokens.AddAsync(resetPasswordToken);
            await applicationDbContext.SaveChangesAsync();

            return resetPasswordToken;
        }

        public async Task<ActionToken?> GetActionTokenAsync(Guid userId, ActionTokenPurpose actionTokenPurpose)
        {
            var actionToken = await applicationDbContext.ActionTokens.Where(x => x.UserId == userId && !x.IsUsed && x.ExpiresAtUtc > DateTime.UtcNow && x.Purpose == actionTokenPurpose)
                                                        .OrderByDescending(x => x.CreatedAtUtc)
                                                        .FirstOrDefaultAsync();
            return actionToken;
        }

        public async Task UpdateActionTokenAsync(ActionToken resetPasswordToken)
        {
            applicationDbContext.ActionTokens.Update(resetPasswordToken);
            await applicationDbContext.SaveChangesAsync();
        }
    }
}
