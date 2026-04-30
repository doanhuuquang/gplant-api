namespace Gplant.Domain.DTOs.Responses
{
    public record UserPagedResult : PagedResult<UserResponse>
    {
        public int ActiveUsersCount { get; init; }
        public int LockedUsersCount { get; init; }
        public int NewUsersThisWeek { get; init; }
    }
}