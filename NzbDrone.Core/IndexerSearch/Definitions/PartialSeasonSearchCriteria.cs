namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class PartialSeasonSearchCriteria : SeasonSearchCriteria
    {
        public int Prefix { get; set; }

        public PartialSeasonSearchCriteria(SeasonSearchCriteria seasonSearch, int prefix)
        {
            Prefix = prefix;
            SceneTitle = seasonSearch.SceneTitle;
            SeasonNumber = seasonSearch.SeasonNumber;
        }

        public override string ToString()
        {
            return string.Format("[{0} : S{1:00}E{1:0}*]", SceneTitle, SeasonNumber, Prefix);
        }
    }
}