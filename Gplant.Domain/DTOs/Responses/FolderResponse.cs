namespace Gplant.Domain.DTOs.Responses
{
    public class FolderResponse
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public int MediaCount { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; }
    }
}