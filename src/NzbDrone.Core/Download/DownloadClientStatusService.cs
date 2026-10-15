using System;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider.Events;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.Download
{
    public interface IDownloadClientStatusService : IProviderStatusServiceBase<DownloadClientStatus>
    {
        void RecordSubmissionFailure(int providerId);
        void RecordSubmissionSuccess(int providerId);
    }

    public class DownloadClientStatusService : ProviderStatusServiceBase<IDownloadClient, DownloadClientStatus>, IDownloadClientStatusService
    {
        public DownloadClientStatusService(IDownloadClientStatusRepository providerStatusRepository, IEventAggregator eventAggregator, IRuntimeInfo runtimeInfo, Logger logger)
            : base(providerStatusRepository, eventAggregator, runtimeInfo, logger)
        {
            MinimumTimeSinceInitialFailure = TimeSpan.FromMinutes(5);
            MaximumEscalationLevel = 5;
        }

        public void RecordSubmissionFailure(int providerId)
        {
            if (providerId <= 0)
            {
                return;
            }

            lock (_syncRoot)
            {
                var status = GetProviderStatus(providerId);
                status.SubmissionFailureCount = Math.Min(3, status.SubmissionFailureCount + 1);
                status.MostRecentSubmissionFailure = DateTime.UtcNow;

                _providerStatusRepository.Upsert(status);
                _eventAggregator.PublishEvent(new ProviderStatusChangedEvent<IDownloadClient>(providerId, status));
            }
        }

        public void RecordSubmissionSuccess(int providerId)
        {
            if (providerId <= 0)
            {
                return;
            }

            lock (_syncRoot)
            {
                var status = GetProviderStatus(providerId);

                if (status.SubmissionFailureCount == 0)
                {
                    return;
                }

                status.SubmissionFailureCount = 0;
                status.MostRecentSubmissionFailure = null;

                _providerStatusRepository.Upsert(status);
                _eventAggregator.PublishEvent(new ProviderStatusChangedEvent<IDownloadClient>(providerId, status));
            }
        }
    }
}
