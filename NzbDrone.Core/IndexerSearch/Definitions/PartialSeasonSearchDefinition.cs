namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class PartialSeasonSearchDefinition : SeasonSearchDefinition
    {
        public int Prefix { get; set; }

        public PartialSeasonSearchDefinition(SeasonSearchDefinition seasonSearch, int prefix)
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