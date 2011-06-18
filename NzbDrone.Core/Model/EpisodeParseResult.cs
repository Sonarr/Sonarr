using System;
using System.Collections.Generic;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Model
{
    public class EpisodeParseResult
    {
        internal string CleanTitle { get; set; }

        internal int SeasonNumber { get; set; }

        internal List<int> EpisodeNumbers { get; set; }

        internal DateTime AirDate { get; set; }
        
        public Quality Quality { get; set; }

        public LanguageType Language { get; set; }

        public string NzbUrl { get; set; }

        public string NzbTitle { get; set; }

        public Series Series { get; set; }

        public IList<Episode> Episodes { get; set; }

        public String Indexer { get; set; }

        public override string ToString()
        {
            if (EpisodeNumbers == null)
                return string.Format("{0} - {1} {2}", CleanTitle, AirDate.ToShortDateString(), Quality);

            return string.Format("{0} - S{1:00}E{2} {3}", CleanTitle, SeasonNumber,
                                 String.Join("-", EpisodeNumbers),Quality);

        }
    }
}