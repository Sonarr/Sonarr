using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace NzbDrone.Core.Indexers
{
    public static class XmlCleaner
    {

        private static readonly Regex ReplaceEntitiesRegex = new Regex("&[a-z]+;", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ReplaceUnicodeRegex = new Regex(@"[^\x09\x0A\x0D\x20-\xD7FF\xE000-\xFFFD\x10000-x10FFFF]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string ReplaceEntities(string content)
        {
            return ReplaceEntitiesRegex.Replace(content, ReplaceEntity);
        }

        public static string ReplaceUnicode(string content)
        {
            return ReplaceUnicodeRegex.Replace(content, string.Empty);
        }

        private static string ReplaceEntity(Match match)
        {
            try
            {
                var character = WebUtility.HtmlDecode(match.Value);
                return string.Concat("&#", (int)character[0], ";");
            }
            catch
            {
                return match.Value;
            }
        }
    }
}
