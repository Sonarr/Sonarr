using System;
using System.Text.RegularExpressions;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.HealthCheck
{
    public class HealthCheck : ModelBase
    {
        private static readonly Regex CleanFragmentRegex = new Regex("[^a-z ]", RegexOptions.Compiled);

        public Type Source { get; set; }
        public HealthCheckResult Type { get; set; }
        public string Message { get; set; }
        public Uri WikiUrl { get; set; }

        public HealthCheck(Type source)
        {
            Source = source;
            Type = HealthCheckResult.Ok;
        }

        public HealthCheck(Type source, HealthCheckResult type, string message, string wikiFragment = null)
        {
            Source = source;
            Type = type;
            Message = message;
            WikiUrl = MakeWikiUrl(wikiFragment ?? MakeWikiFragment(message));
        }

        private static string MakeWikiFragment(string message)
        {
            return "#" + CleanFragmentRegex.Replace(message.ToLower(), string.Empty).Replace(' ', '-');
        }

        private static Uri MakeWikiUrl(string fragment)
        {
            var rootUri = new Uri("https://github.com/Sonarr/Sonarr/wiki/Health-checks");
            if (fragment.StartsWith("#"))
            { // Mono doesn't understand # and generates a different url than windows.
                return new Uri(rootUri + fragment);
            }
            else
            {
                var fragmentUri = new Uri(fragment, UriKind.Relative);

                return new Uri(rootUri, fragmentUri);
            }
        }
    }

    public enum HealthCheckResult
    {
        Ok = 0,
        Warning = 1,
        Error = 2
    }
}
