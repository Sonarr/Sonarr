using System;

namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class DailySeasonSearchCriteria : SearchCriteriaBase
    {
        public int Year { get; set; }

        public override string ToString()
        {
            return string.Format("[{0} : {1}]", Series.Title, Year);
        }
    }
}
