using NzbDrone.Core.Indexers;

namespace NzbDrone.Api.Indexers
{
    public class IndexerResource : ProviderResource
    {
        public bool EnableRss { get; set; }
        public bool EnableSearch { get; set; }
        public bool SupportsRss { get; set; }
        public bool SupportsSearch { get; set; }
        public DownloadProtocol Protocol { get; set; }
    }
}
