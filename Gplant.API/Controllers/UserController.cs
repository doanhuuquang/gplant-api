using Gplant.API.ApiResponse;
using Gplant.Application.Interfaces;
using Gplant.Domain.DTOs.Requests.User;
using Gplant.Domain.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gplant.API.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController(IUserService userService) : ControllerBase
    {
        /// <summary>
        /// Get current user info
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await userService.GetCurrentUserAsync(User);

            var response = new SuccessResponse<UserResponse>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get current user successful.",
                Data: user,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Update own profile
        /// </summary>
        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UpdateUserRequest request)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await userService.UpdateUserAsync(userId, request);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Update profile successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get all users (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAllUsers([FromQuery] UserFilterRequest filter)
        {
            var result = await userService.GetAllUsersAsync(filter);

            var response = new SuccessResponse<PagedResult<UserResponse>>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get all users successful.",
                Data: result,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Get user by ID (Admin only)
        /// </summary>
        [HttpGet("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await userService.GetUserByIdAsync(id);

            var response = new SuccessResponse<UserResponse>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Get user successful.",
                Data: user,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Update user info (Admin only)
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateUser(Guid id, UpdateUserRequest request)
        {
            await userService.UpdateUserAsync(id, request);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Update user successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Delete user (Admin only)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            // Không cho xóa chính mình
            var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (id == currentUserId)
                return BadRequest(new ErrorResponse(
                    StatusCode: 400,
                    Error: "BadRequest",
                    Message: "Cannot delete your own account",
                    Timestamp: DateTime.UtcNow
                ));

            await userService.DeleteUserAsync(id);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Delete user successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Assign role to user (Admin only)
        /// </summary>
        [HttpPost("{id:guid}/roles")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AssignRole(Guid id, AssignRoleRequest request)
        {
            var user = await userService.AssignRoleAsync(id, request.RoleName);

            var response = new SuccessResponse<UserResponse>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Assign role successful.",
                Data: user,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Remove role from user (Admin only)
        /// </summary>
        [HttpDelete("{id:guid}/roles/{roleName}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> RemoveRole(Guid id, string roleName)
        {
            var user = await userService.RemoveRoleAsync(id, roleName);

            var response = new SuccessResponse<UserResponse>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Remove role successful.",
                Data: user,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }

        /// <summary>
        /// Lock/Unlock user account (Admin only)
        /// </summary>
        [HttpPatch("{id:guid}/toggle-lock")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ToggleLock(Guid id)
        {
            // Không cho khóa chính mình
            var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (id == currentUserId)
                return BadRequest(new ErrorResponse(
                    StatusCode: 400,
                    Error: "BadRequest",
                    Message: "Cannot lock your own account",
                    Timestamp: DateTime.UtcNow
                ));

            await userService.ToggleLockUserAsync(id);

            var response = new SuccessResponse<object?>(
                StatusCode: StatusCodes.Status200OK,
                Message: "Toggle lock user successful.",
                Data: null,
                Timestamp: DateTime.UtcNow
            );

            return Ok(response);
        }
    }
}
