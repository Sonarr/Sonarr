using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Model
{
    public class EpisodeParseResult
    {
        public string SeriesTitle { get; set; }
        public string CleanTitle
        {
            get
            {
                return Parser.NormalizeTitle(SeriesTitle);
            }
        }

        public string EpisodeTitle { get; set; }

        public int SeasonNumber { get; set; }

        public List<int> EpisodeNumbers { get; set; }

        public DateTime? AirDate { get; set; }

        public Quality Quality { get; set; }

        public LanguageType Language { get; set; }

        public string NzbUrl { get; set; }

        public string NzbInfoUrl { get; set; }

        public string OriginalString { get; set; }

        public Series Series { get; set; }

        public String Indexer { get; set; }

        public bool FullSeason { get; set; }

        public long Size { get; set; }

        public int Age { get; set; }

        public string ReleaseGroup { get; set; }

        public bool SceneSource { get; set; }

        public IList<Episode> Episodes { get; set; } 

        public override string ToString()
        {

            string episodeString = "[Unknown Episode]";

            if (AirDate != null && EpisodeNumbers == null)
            {
                episodeString = string.Format("{0}", AirDate.Value.ToString("yyyy-MM-dd"));
            }
            else if (FullSeason)
            {
                episodeString = string.Format("Season {0:00}", SeasonNumber);
            }
            else if (EpisodeNumbers != null && EpisodeNumbers.Any())
            {
                episodeString = string.Format("S{0:00}E{1}",SeasonNumber, String.Join("-", EpisodeNumbers.Select(c => c.ToString("00"))));
            }

            return string.Format("{0} - {1} {2}", SeriesTitle, episodeString, Quality);

        }
    }
}