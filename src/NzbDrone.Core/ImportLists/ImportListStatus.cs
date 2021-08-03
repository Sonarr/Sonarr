using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.ImportLists
{
    public class ImportListStatus : ProviderStatusBase
    {
        public ImportListItemInfo LastSyncListInfo { get; set; }
    }
}
