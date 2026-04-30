using Microsoft.AspNetCore.Http;

namespace Gplant.Domain.DTOs.Requests.Media
{
    public class UploadImageRequest
    {
        public required IFormFile File { get; set; }
        public required Guid FolderId { get; set; }
        public string? Folder { get; set; } = "general";
    }
}