namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class AnimeEpisodeSearchCriteria : SearchCriteriaBase
    {
        public int AbsoluteEpisodeNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
        public bool IsSeasonSearch { get; set; }

        public override string ToString()
        {
            return $"[{Series.Title} : S{SeasonNumber:00}E{EpisodeNumber:00} ({AbsoluteEpisodeNumber:00})]";
        }
    }
}
