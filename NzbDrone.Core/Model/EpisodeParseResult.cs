using System;
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

        public string OriginalString { get; set; }

        public Series Series { get; set; }

        public String Indexer { get; set; }

        public bool FullSeason { get; set; }

        public long Size { get; set; }

        public int NewzbinId { get; set; }

        public override string ToString()
        {
            if (AirDate != null && EpisodeNumbers == null)
                return string.Format("{0} - {1} {2}", SeriesTitle, AirDate.Value.ToShortDateString(), Quality);

            if (FullSeason)
                return string.Format("{0} - Season {1:00}", SeriesTitle, SeasonNumber);

            if (EpisodeNumbers != null)
                return string.Format("{0} - S{1:00}E{2} {3}", SeriesTitle, SeasonNumber,
                                     String.Join("-", EpisodeNumbers), Quality);

            return OriginalString;

        }
    }
}