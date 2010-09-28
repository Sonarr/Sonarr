using System;
using System.Text.RegularExpressions;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public class EpisodeProvider
    {
        private static Regex _parseRegex =
    new Regex(
        @"(?<showName>.*)
(?:
  s(?<seasonNumber>\d+)e(?<episodeNumber>\d+)-?e(?<episodeNumber2>\d+)
| s(?<seasonNumber>\d+)e(?<episodeNumber>\d+)
|  (?<seasonNumber>\d+)x(?<episodeNumber>\d+)
|  (?<airDate>\d{4}.\d{2}.\d{2})
)
(?:
  (?<episodeName>.*?)
  (?<release>
     (?:hdtv|pdtv|xvid|ws|720p|x264|bdrip|dvdrip|dsr|proper)
     .*)
| (?<episodeName>.*)
)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);


        public static Episode Parse(string title)
        {
            Match match = _parseRegex.Match(title);

            if (!match.Success)
                return null;

            return new Episode
            {
                
                Season = ParseInt(match.Groups["seasonNumber"].Value),
                EpisodeNumber = ParseInt(match.Groups["episodeNumber"].Value),
                EpisodeNumber2 = ParseInt(match.Groups["episodeNumber2"].Value),
                Title = ReplaceSeparatorChars(match.Groups["episodeName"].Value),
                Release = ReplaceSeparatorChars(match.Groups["release"].Value),
                Proper = title.Contains("PROPER")
            };
        }

        private static string ReplaceSeparatorChars(string s)
        {
            if (s == null) return string.Empty;
            return s.Replace('.', ' ').Replace('-', ' ').Replace('_', ' ').Trim();
        }

        private static int ParseInt(string s)
        {
            int i;
            int.TryParse(s, out i);
            return i;
        }

        private static DateTime ParseAirDate(string s)
        {
            DateTime d;
            if (DateTime.TryParse(ReplaceSeparatorChars(s).Replace(' ', '-'), out d))
                return d;
            return DateTime.MinValue;
        }

    }
}