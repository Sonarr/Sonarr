using System;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class SpecialEpisodeSearchCriteria : SeriesSearchCriteriaBase
    {
        public string[] EpisodeQueryTitles { get; set; }

        public override string ToString()
        {
            return string.Format("[{0} : {1}]", Media.Title, String.Join(",", EpisodeQueryTitles));
        }
    }
}
