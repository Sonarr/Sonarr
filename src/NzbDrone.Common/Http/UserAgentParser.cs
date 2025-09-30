using System.Text.RegularExpressions;

namespace NzbDrone.Common.Http
{
    public static class UserAgentParser
    {
        private static readonly Regex AppSourceRegex = new(@"(?<agent>[a-z0-9]*)\/.*(?:\(.*\))?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string SimplifyUserAgent(string userAgent)
        {
            if (userAgent == null || userAgent.StartsWith("Mozilla/5.0"))
            {
                return null;
            }

            return userAgent;
        }

        public static string ParseSource(string userAgent)
        {
            var match = AppSourceRegex.Match(SimplifyUserAgent(userAgent) ?? string.Empty);

            if (match.Groups["agent"].Success)
            {
                return match.Groups["agent"].Value;
            }

            return "Other";
        }
    }
}
