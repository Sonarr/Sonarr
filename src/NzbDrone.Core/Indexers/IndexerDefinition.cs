using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public class IndexerDefinition : ProviderDefinition
    {
        public DownloadProtocol Protocol { get; set; }
    }
}
