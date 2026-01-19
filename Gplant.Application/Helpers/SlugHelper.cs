using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Gplant.Application.Helpers
{
    public partial class SlugHelper
    {
        [GeneratedRegex(@"\s+")]
        private static partial Regex MyRegex();

        [GeneratedRegex(@"[^a-z0-9\-]")]
        private static partial Regex MyRegex1();

        public static string GenerateSlug(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            input = input.ToLower();
            input = input.Normalize(NormalizationForm.FormD);

            var sb = new StringBuilder();
            foreach (char c in input)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            var result  = sb.ToString().Normalize(NormalizationForm.FormC);
            result      = MyRegex().Replace(result, "-");
            result      = MyRegex1().Replace(result, "");

            return result;
        }
    }
}
