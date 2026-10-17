using System.Text.RegularExpressions;

namespace NzbDrone.Core.ImportLists.Trakt.SmartList
{
    public static class TraktSmartListSlug
    {
        private static readonly Regex SmartListUrlRegex = new(@"/(?:lists/smart/view|smart-lists)/(?<slug>[^/?#]+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SlugRegex = new(@"^[\w-]+$", RegexOptions.Compiled);

        public static string Parse(string smartList)
        {
            var value = smartList.Trim();
            var match = SmartListUrlRegex.Match(value);

            return match.Success ? match.Groups["slug"].Value : value.Trim('/');
        }

        public static bool IsValid(string smartList)
        {
            return smartList != null && SlugRegex.IsMatch(Parse(smartList));
        }
    }
}
