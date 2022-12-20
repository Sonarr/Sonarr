using System;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.ImportLists
{
    public interface IImportListStatusService : IProviderStatusServiceBase<ImportListStatus>
    {
        DateTime? GetLastSyncListInfo(int importListId);

        void UpdateListSyncStatus(int importListId);
    }

    public class ImportListStatusService : ProviderStatusServiceBase<IImportList, ImportListStatus>, IImportListStatusService
    {
        public ImportListStatusService(IImportListStatusRepository providerStatusRepository, IEventAggregator eventAggregator, IRuntimeInfo runtimeInfo, Logger logger)
            : base(providerStatusRepository, eventAggregator, runtimeInfo, logger)
        {
        }

        public DateTime? GetLastSyncListInfo(int importListId)
        {
            return GetProviderStatus(importListId).LastInfoSync;
        }

        public void UpdateListSyncStatus(int importListId)
        {
            lock (_syncRoot)
            {
                var status = GetProviderStatus(importListId);

                status.LastInfoSync = DateTime.UtcNow;

                _providerStatusRepository.Upsert(status);
            }
        }
    }
}
