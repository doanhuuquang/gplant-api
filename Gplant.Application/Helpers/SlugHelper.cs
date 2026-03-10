using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Gplant.Application.Helpers
{
    public partial class SlugHelper
    {
        [GeneratedRegex(@"\s+")]
        private static partial Regex WhitespaceRegex();

        [GeneratedRegex(@"[^a-z0-9\-]")]
        private static partial Regex InvalidCharsRegex();

        [GeneratedRegex(@"-{2,}")]
        private static partial Regex MultipleHyphensRegex();


        public static string GenerateSlug(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var slug = input.ToLowerInvariant();
            slug = WhitespaceRegex().Replace(slug, "-");
            slug = InvalidCharsRegex().Replace(slug, "");
            slug = MultipleHyphensRegex().Replace(slug, "-");
            slug = slug.Trim('-');

            return slug;
        }
    }
}
