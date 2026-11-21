using System;
using System.Text.RegularExpressions;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.ImportLists.MDBList
{
    public static class MDBListHelper
    {
        private static readonly Regex ListUrlRegex = new(@"(?:^|/)lists/(?<user>[^/?#\s]+)/(?<slug>[^/?#\s]+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string GetListPath(string list)
        {
            if (list.IsNullOrWhiteSpace())
            {
                return null;
            }

            var match = ListUrlRegex.Match(list);

            if (!match.Success)
            {
                return null;
            }

            return $"lists/{Uri.EscapeDataString(Uri.UnescapeDataString(match.Groups["user"].Value))}/{Uri.EscapeDataString(Uri.UnescapeDataString(match.Groups["slug"].Value))}";
        }
    }
}
