using System;
using System.Linq;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser.Model
{
    public class ParsedEpisodeInfo
    {
        public string SeriesTitle { get; set; }
        public SeriesTitleInfo SeriesTitleInfo { get; set; }
        public QualityModel Quality { get; set; }
        public int SeasonNumber { get; set; }
        public int[] EpisodeNumbers { get; set; }
        public int[] AbsoluteEpisodeNumbers { get; set; }
        public String AirDate { get; set; }
        public Language Language { get; set; }
        
        public bool FullSeason { get; set; }

        public override string ToString()
        {
            string episodeString = "[Unknown Episode]";

            if (IsDaily() && EpisodeNumbers == null)
            {
                episodeString = string.Format("{0}", AirDate);
            }
            else if (FullSeason)
            {
                episodeString = string.Format("Season {0:00}", SeasonNumber);
            }
            else if (EpisodeNumbers != null && EpisodeNumbers.Any())
            {
                episodeString = string.Format("S{0:00}E{1}", SeasonNumber, String.Join("-", EpisodeNumbers.Select(c => c.ToString("00"))));
            }

            return string.Format("{0} - {1} {2}", SeriesTitle, episodeString, Quality);
        }

        public bool IsDaily()
        {
            return !String.IsNullOrWhiteSpace(AirDate);
        }
    }
}