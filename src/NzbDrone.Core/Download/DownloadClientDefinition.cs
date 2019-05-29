using NzbDrone.Core.Indexers;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Download
{
    public class DownloadClientDefinition : ProviderDefinition
    {
        public DownloadProtocol Protocol { get; set; }
        public int Priority { get; set; } = 1;
    }
}
