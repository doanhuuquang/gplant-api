using Microsoft.EntityFrameworkCore;
using Gplant.Application.Abstracts;
using Gplant.Domain.Entities;

namespace Gplant.Infrastructure.Repositories
{
    public class UserRepository(ApplicationDbContext applicationDbContext) : IUserRepository
    {
        public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
        {
            var user = await applicationDbContext.Users.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);

            return user;
        }
    }
}
