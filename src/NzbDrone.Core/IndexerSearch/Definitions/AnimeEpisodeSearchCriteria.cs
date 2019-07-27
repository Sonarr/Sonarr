namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class AnimeEpisodeSearchCriteria : SearchCriteriaBase
    {
        public int AbsoluteEpisodeNumber { get; set; }
        public bool IsSeasonSearch { get; set; }

        public override string ToString()
        {
            return string.Format("[{0} : {1:00}]", Series.Title, AbsoluteEpisodeNumber);
        }
    }
}