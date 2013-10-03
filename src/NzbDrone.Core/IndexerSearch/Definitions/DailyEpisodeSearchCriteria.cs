using System;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class DailyEpisodeSearchCriteria : SearchCriteriaBase
    {
        public DateTime Airtime { get; set; }

        public override string ToString()
        {
            return string.Format("[{0} : {1}", SceneTitle, Airtime);
        }
    }
}