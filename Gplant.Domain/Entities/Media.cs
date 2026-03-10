namespace Gplant.Domain.Entities
{
    public class Media
    {
        public Guid Id { get; set; }
        public required string FileName { get; set; }
        public required string FilePath { get; set; }
        public required string FileUrl { get; set; }
        public long FileSize { get; set; }
        public required string MimeType { get; set; }
        public string? FileHash { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public Guid UploadedBy { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
    }
}