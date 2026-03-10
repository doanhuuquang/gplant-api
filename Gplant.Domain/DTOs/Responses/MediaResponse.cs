namespace Gplant.Domain.DTOs.Responses
{
    public record MediaResponse
    {
        public Guid Id { get; init; }
        public string FileName { get; init; } = string.Empty;
        public string FileUrl { get; init; } = string.Empty;
        public long FileSize { get; init; }
        public string MimeType { get; init; } = string.Empty;
        public int? Width { get; init; }
        public int? Height { get; init; }
        public required UserResponse UploadedBy { get; init; }
        public DateTime CreatedAtUtc { get; init; }
    }
}