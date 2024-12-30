using Workarr.Parser.Model;
using Workarr.ThingiProvider.Status;

namespace Workarr.Indexers
{
    public class IndexerStatus : ProviderStatusBase
    {
        public ReleaseInfo LastRssSyncReleaseInfo { get; set; }
    }
}
