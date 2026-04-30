using Gplant.Domain.DTOs.Requests.User;
using Gplant.Domain.DTOs.Responses;
using System.Security.Claims;

namespace Gplant.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserResponse> GetCurrentUserAsync(ClaimsPrincipal principal);
        Task<UserPagedResult> GetAllUsersAsync(UserFilterRequest filter);
        Task<UserResponse> GetUserByIdAsync(Guid id);
        Task UpdateUserAsync(Guid id, UpdateUserRequest request);
        Task DeleteUserAsync(Guid id);
        Task<UserResponse> AssignRoleAsync(Guid userId, string roleName);
        Task<UserResponse> RemoveRoleAsync(Guid userId, string roleName);
        Task ToggleLockUserAsync(Guid id);
    }
}
