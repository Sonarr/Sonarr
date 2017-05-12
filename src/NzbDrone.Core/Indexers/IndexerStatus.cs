using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.Indexers
{
    public class IndexerStatus : ProviderStatusBase
    {
        public ReleaseInfo LastRssSyncReleaseInfo { get; set; }
    }
}
