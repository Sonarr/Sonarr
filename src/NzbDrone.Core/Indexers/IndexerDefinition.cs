using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public class IndexerDefinition : ProviderDefinition
    {
        public bool EnableRss { get; set; }
        public bool EnableAutomaticSearch { get; set; }
        public bool EnableInteractiveSearch { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public bool SupportsRss { get; set; }
        public bool SupportsSearch { get; set; }

        public override bool Enable => EnableRss || EnableAutomaticSearch || EnableInteractiveSearch;

        public IndexerStatus Status { get; set; }
    }
}
