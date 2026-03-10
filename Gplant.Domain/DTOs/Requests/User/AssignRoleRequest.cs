namespace Gplant.Domain.DTOs.Requests.User
{
    public class AssignRoleRequest
    {
        public required string RoleName { get; set; }  // "Admin", "Manager", "Customer"
    }
}