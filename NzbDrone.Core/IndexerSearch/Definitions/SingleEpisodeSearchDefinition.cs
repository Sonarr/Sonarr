namespace NzbDrone.Core.IndexerSearch.Definitions
{
    public class SingleEpisodeSearchDefinition : SearchDefinitionBase
    {

        //TODO make sure these are populated with scene if required
        public int EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }

        public override string ToString()
        {
            return string.Format("[{0} : S{1:00}E{2:00} ]", SceneTitle, SeasonNumber, EpisodeNumber);
        }
    }
}