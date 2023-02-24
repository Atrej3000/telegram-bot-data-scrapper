using System.Text;
using System.Text.RegularExpressions;

namespace DataParserService.Extensions
{
    public static partial class StringExtensions
    {
        public static string ToUTF8(this string text)
        {
            return Encoding.UTF8.GetString(Encoding.Default.GetBytes(text));
        }
        public static string CheckNull(this string? text)
        {
            return text ?? string.Empty;
        }
    }
}
