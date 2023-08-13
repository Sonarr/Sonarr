using NzbDrone.Core.ImportLists;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class FixFutureImportListStatusTimes : FixFutureProviderStatusTimes<ImportListStatus>, IHousekeepingTask
    {
        public FixFutureImportListStatusTimes(IImportListStatusRepository importListStatusRepository)
            : base(importListStatusRepository)
        {
        }
    }
}
