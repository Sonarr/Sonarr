using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Tv
{
    public class AddSeriesOptions : IEmbeddedDocument
    {
        public bool SearchForMissingEpisodes { get; set; }
        public bool IgnoreEpisodesWithFiles { get; set; }
        public bool IgnoreEpisodesWithoutFiles { get; set; }
    }
}
