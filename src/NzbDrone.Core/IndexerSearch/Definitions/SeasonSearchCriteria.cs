namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class SeasonSearchCriteria : SearchCriteriaBase
    {
        public int SeasonNumber { get; set; }

        public override bool MonitoredEpisodesOnly
        {
            get
            {
                return true;
            }
        }

        public override string ToString()
        {
            return string.Format("[{0} : S{1:00}]", Series.Title, SeasonNumber);
        }
    }
}