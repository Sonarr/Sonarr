namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class SeasonSearchCriteria : SeriesSearchCriteriaBase
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
            return string.Format("[{0} : S{1:00}]", Media.Title, SeasonNumber);
        }
    }
}