namespace Gplant.Domain.DTOs.Requests.User
{
    public class UserFilterRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchTerm { get; set; }     // Tìm theo email, tên
        public string? Role { get; set; }           // Lọc theo role
        public bool? EmailConfirmed { get; set; }   // Lọc xác thực email
    }
}