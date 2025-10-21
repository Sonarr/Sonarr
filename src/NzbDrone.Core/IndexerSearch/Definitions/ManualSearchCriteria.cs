namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class ManualSearchCriteria : SearchCriteriaBase
    {
        public string SearchQuery { get; set; }

        public override string ToString()
        {
            return string.Format("[{0} : Manual Search - {1}]", Series?.Title ?? "Unknown", SearchQuery);
        }
    }
}
