using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.User;
using Gplant.Domain.DTOs.Responses;
using Gplant.Domain.Entities;
using Gplant.Domain.Exceptions;
using Gplant.Domain.Exceptions.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Gplant.Application.Services
{
    public class UserService(
        UserManager<User> userManager, 
        RoleManager<Role> roleManager,
        IOrderRepository orderRepository,
        IMediaRepository mediaRepository) : IUserService 
    {
        public async Task<UserResponse> GetCurrentUserAsync(ClaimsPrincipal principal)
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
            var user = await userManager.FindByIdAsync(userId) ?? throw new UserException("User not found.");

            return await MapToResponseAsync(user);
        }

        public async Task<UserPagedResult> GetAllUsersAsync(UserFilterRequest filter)
        {
            var query = userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchLower = filter.SearchTerm.ToLower();
                query = query.Where(u =>
                    u.Email!.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase) ||
                    u.UserName!.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase) ||
                    (u.FirstName != null && u.FirstName.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase)) ||
                    (u.LastName != null && u.LastName.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase))
                );
            }

            if (filter.EmailConfirmed.HasValue)
            {
                query = query.Where(u => u.EmailConfirmed == filter.EmailConfirmed.Value);
            }

            var totalCount = await query.CountAsync();
            var activeUsersCount = await query.CountAsync(u => !u.LockoutEnd.HasValue || u.LockoutEnd <= DateTimeOffset.UtcNow);
            var lockedUsersCount = totalCount - activeUsersCount;
            var newUsersThisWeek = await query.CountAsync(u => u.CreatedAtUtc >= DateTime.UtcNow.AddDays(-7));

            var users = await query.OrderBy(u => u.Email)
                                   .Skip((filter.PageNumber - 1) * filter.PageSize)
                                   .Take(filter.PageSize)
                                   .ToListAsync();

            var userResponses = new List<UserResponse>();
            foreach (var user in users)
            {
                var userResponse = await MapToResponseAsync(user);
                if (!string.IsNullOrWhiteSpace(filter.Role))
                {
                    if (userResponse.Roles.Contains(filter.Role))
                        userResponses.Add(userResponse);
                }
                else
                {
                    userResponses.Add(userResponse);
                }
            }

            return new UserPagedResult
            {
                Items = userResponses,
                TotalCount = totalCount,
                ActiveUsersCount = activeUsersCount,
                LockedUsersCount = lockedUsersCount,
                NewUsersThisWeek = newUsersThisWeek,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<UserResponse> GetUserByIdAsync(Guid id)
        {
            var user = await userManager.FindByIdAsync(id.ToString()) ?? throw new UserNotFoundException($"User with ID {id} not found");
            return await MapToResponseAsync(user);
        }

        public async Task UpdateUserAsync(Guid id, UpdateUserRequest request)
        {
            var user = await userManager.FindByIdAsync(id.ToString())
                ?? throw new UserNotFoundException($"User with ID {id} not found");

            if (!string.IsNullOrWhiteSpace(request.FirstName))
                user.FirstName = request.FirstName;

            if (!string.IsNullOrWhiteSpace(request.LastName))
                user.LastName = request.LastName;

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                user.PhoneNumber = request.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(request.ProfilePictureUrl))
                user.ProfilePictureUrl = request.ProfilePictureUrl;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new UserException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var user = await userManager.FindByIdAsync(id.ToString())
                ?? throw new UserNotFoundException($"User with ID {id} not found");

            // ✅ Kiểm tra có đơn hàng không
            var hasOrders = await orderRepository.HasOrdersByUserIdAsync(id);
            if (hasOrders) throw new UserException("Cannot delete user with existing orders. User has order history.");

            // ✅ Kiểm tra có media đã upload không
            var hasMedia = await mediaRepository.HasMediaByUserIdAsync(id);
            if (hasMedia) throw new UserException("Cannot delete user who has uploaded media. Please reassign or delete the media first.");

            // Cart sẽ tự động xóa (cascade)

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded) throw new UserException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        public async Task<UserResponse> AssignRoleAsync(Guid userId, string roleName)
        {
            var user = await userManager.FindByIdAsync(userId.ToString())
                ?? throw new UserNotFoundException($"User with ID {userId} not found");

            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
                throw new UserException($"Role '{roleName}' not found");

            var isInRole = await userManager.IsInRoleAsync(user, roleName);
            if (isInRole)
                throw new UserException($"User already has role '{roleName}'");

            var result = await userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
                throw new UserException(string.Join(", ", result.Errors.Select(e => e.Description)));

            return await MapToResponseAsync(user);
        }

        public async Task<UserResponse> RemoveRoleAsync(Guid userId, string roleName)
        {
            var user = await userManager.FindByIdAsync(userId.ToString())
                ?? throw new UserNotFoundException($"User with ID {userId} not found");

            var isInRole = await userManager.IsInRoleAsync(user, roleName);
            if (!isInRole)
                throw new UserException($"User does not have role '{roleName}'");

            var userRoles = await userManager.GetRolesAsync(user);
            if (userRoles.Count == 1)
                throw new UserException("Cannot remove the last role. User must have at least one role.");

            var result = await userManager.RemoveFromRoleAsync(user, roleName);
            if (!result.Succeeded)
                throw new UserException(string.Join(", ", result.Errors.Select(e => e.Description)));

            return await MapToResponseAsync(user);
        }

        public async Task ToggleLockUserAsync(Guid id)
        {
            var user = await userManager.FindByIdAsync(id.ToString())
                ?? throw new UserNotFoundException($"User with ID {id} not found");

            var isLocked = await userManager.IsLockedOutAsync(user);

            if (isLocked)
            {
                // Mở khóa
                user.LockoutEnd = null;
            }
            else
            {
                // Khóa vĩnh viễn
                user.LockoutEnd = DateTimeOffset.MaxValue;
            }

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new UserException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // Helper
        private async Task<UserResponse> MapToResponseAsync(User user)
        {
            var roles = await userManager.GetRolesAsync(user);
            var isLocked = await userManager.IsLockedOutAsync(user);

            return new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                ProfilePictureUrl = user.ProfilePictureUrl ?? "",
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber ?? "",
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                Roles = [.. roles],
                IsLocked = isLocked,                
                LockoutEnd = user.LockoutEnd    
            };
        }
    }
}
