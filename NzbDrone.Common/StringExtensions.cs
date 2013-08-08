using System.Text;
using System.Text.RegularExpressions;

namespace NzbDrone.Common
{
    public static class StringExtensions
    {
        public static string Inject(this string format, params object[] formattingArgs)
        {
            return string.Format(format, formattingArgs);
        }

        private static readonly Regex InvalidCharRegex = new Regex(@"[^a-zA-Z0-9\s-]", RegexOptions.Compiled);
        private static readonly Regex InvalidSearchCharRegex = new Regex(@"[^a-zA-Z0-9\s-\.]", RegexOptions.Compiled);
        private static readonly Regex CollapseSpace = new Regex(@"\s+", RegexOptions.Compiled);

        public static string ToSlug(this string phrase)
        {
            phrase = phrase.RemoveAccent().ToLower();

            phrase = InvalidCharRegex.Replace(phrase, string.Empty);
            phrase = CollapseSpace.Replace(phrase, " ").Trim();
            phrase = phrase.Replace(" ", "-");

            return phrase;
        }

        public static string ToSearchTerm(this string phrase)
        {
            phrase = phrase.RemoveAccent().ToLower();

            phrase = phrase.Replace("&", "and");
            phrase = InvalidSearchCharRegex.Replace(phrase, string.Empty);
            phrase = CollapseSpace.Replace(phrase, " ").Trim();
            phrase = phrase.Replace(" ", "+");
            
            return phrase;
        }

        public static string RemoveAccent(this string txt)
        {
            var bytes = Encoding.GetEncoding("Cyrillic").GetBytes(txt);
            return Encoding.ASCII.GetString(bytes);
        }
    }
}