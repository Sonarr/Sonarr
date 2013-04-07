namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class SeasonSearchDefinition : SearchDefinitionBase
    {
        public int SeasonNumber { get; set; }

        public override string ToString()
        {
            return string.Format("[{0} : S{1:00}]", SceneTitle, SeasonNumber);
        }
    }
}