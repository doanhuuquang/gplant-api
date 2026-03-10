using Microsoft.AspNetCore.Http;

namespace Gplant.Domain.DTOs.Requests.Media
{
    public record UploadImageRequest
    {
        public required IFormFile File { get; init; }
        public string Folder { get; init; } = "general";
    }
}