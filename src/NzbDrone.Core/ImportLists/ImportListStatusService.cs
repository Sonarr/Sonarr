using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.ImportLists
{
    public interface IImportListStatusService : IProviderStatusServiceBase<ImportListStatus>
    {
        ImportListStatus GetListStatus(int importListId);

        void UpdateListSyncStatus(int importListId, bool removedItems);
        void MarkListsAsCleaned();
    }

    public class ImportListStatusService : ProviderStatusServiceBase<IImportList, ImportListStatus>, IImportListStatusService
    {
        public ImportListStatusService(IImportListStatusRepository providerStatusRepository, IEventAggregator eventAggregator, IRuntimeInfo runtimeInfo, Logger logger)
            : base(providerStatusRepository, eventAggregator, runtimeInfo, logger)
        {
        }

        public ImportListStatus GetListStatus(int importListId)
        {
            return GetProviderStatus(importListId);
        }

        public void UpdateListSyncStatus(int importListId, bool removedItems)
        {
            lock (_syncRoot)
            {
                var status = GetProviderStatus(importListId);

                status.LastInfoSync = DateTime.UtcNow;
                status.HasRemovedItemSinceLastClean |= removedItems;

                _providerStatusRepository.Upsert(status);
            }
        }

        public void MarkListsAsCleaned()
        {
            lock (_syncRoot)
            {
                var toUpdate = new List<ImportListStatus>();

                foreach (var status in _providerStatusRepository.All())
                {
                    status.HasRemovedItemSinceLastClean = false;
                    toUpdate.Add(status);
                }

                _providerStatusRepository.UpdateMany(toUpdate);
            }
        }
    }
}
