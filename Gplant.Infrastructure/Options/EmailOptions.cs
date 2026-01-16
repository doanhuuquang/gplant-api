namespace Gplant.Infrastructure.Options
{
    public class EmailOptions
    {
        public const string EmailOptionsKey = "EmailOptions";

        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Host { get; set; }
        public required string Port { get; set; }
    }
}
