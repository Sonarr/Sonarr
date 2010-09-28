using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NzbDrone.Core.Repository
{
    public class Episode
    {
        //Information about the current episode being processed

        private const string EpisodePattern = @"
(?<showName>.*)
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
)
";
        public FeedItem FeedItem { get; set; }
        public string ShowName { get; set; }
        public string EpisodeName { get; set; }
        public string EpisodeName2 { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public int EpisodeNumber2 { get; set; }
        public DateTime AirDate { get; set; }
        public string Release { get; set; }
        public int Quality { get; set; }

        public bool IsProper
        {
            get { return FeedItem.Title.Contains("PROPER"); }
        }

        public bool IsDaily
        {
            get { return AirDate != DateTime.MinValue; }
        }

        public bool IsMulti
        {
            get { return SeasonNumber != 0 && EpisodeNumber != 0 && EpisodeNumber2 != 0; }
        }

        public string EpisodeTitle
        {
            get { return IsDaily ? GetDailyEpisode() : IsMulti ? GetMultiEpisode() : GetSeasonEpisode(); }
        }

        public string Title
        {
            get { return string.Format("{0} - {1}", ShowName, EpisodeTitle); }
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", ShowName, EpisodeTitle);
        }

        private string GetDailyEpisode()
        {
            return AirDate.ToString("yyyy-MM-dd");
        }

        private string GetMultiEpisode()
        {
            return string.Format("{0}x{1:D2}-{0}x{2:D2}", SeasonNumber, EpisodeNumber, EpisodeNumber2);
        }

        private string GetSeasonEpisode()
        {
            return string.Format("{0}x{1:D2}", SeasonNumber, EpisodeNumber);
        }

        public static Episode Parse(FeedItem feedItem)
        {
            Match match = Regex.Match(feedItem.Title, EpisodePattern,
                RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);

            if (!match.Success)
                return null;

            return new Episode
            {
                ShowName = ReplaceSeparatorChars(match.Groups["showName"].Value),
                SeasonNumber = ParseInt(match.Groups["seasonNumber"].Value),
                EpisodeNumber = ParseInt(match.Groups["episodeNumber"].Value),
                EpisodeNumber2 = ParseInt(match.Groups["episodeNumber2"].Value),
                EpisodeName = ReplaceSeparatorChars(match.Groups["episodeName"].Value),
                Release = ReplaceSeparatorChars(match.Groups["release"].Value)
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
