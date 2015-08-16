namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class AnimeEpisodeSearchCriteria : SeriesSearchCriteriaBase
    {
        public int AbsoluteEpisodeNumber { get; set; }

        public override string ToString()
        {
            return string.Format("[{0} : {1:00}]", Media.Title, AbsoluteEpisodeNumber);
        }
    }
}