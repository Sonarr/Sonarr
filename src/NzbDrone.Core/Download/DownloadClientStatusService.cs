using System;
using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.Download
{
    public interface IDownloadClientStatusService : IProviderStatusServiceBase<DownloadClientStatus>
    {

    }

    public class DownloadClientStatusService : ProviderStatusServiceBase<IDownloadClient, DownloadClientStatus>, IDownloadClientStatusService
    {
        public DownloadClientStatusService(IDownloadClientStatusRepository providerStatusRepository, IEventAggregator eventAggregator, Logger logger)
            : base(providerStatusRepository, eventAggregator, logger)
        {
            MinimumTimeSinceInitialFailure = TimeSpan.FromMinutes(5);
            MaximumEscalationLevel = 5;
        }
    }
}
