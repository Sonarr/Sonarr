using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class SpecialEpisodeSearchCriteria : SearchCriteriaBase
    {
        public string[] EpisodeQueryTitles { get; set; }

        public override string ToString()
        {
            return string.Format("[{0} : {1}]", SceneTitle, String.Join(",", EpisodeQueryTitles));
        }
    }
}
