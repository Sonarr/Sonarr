using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Download
{
    public class DownloadProcessingService : IExecute<ProcessMonitoredDownloadsCommand>
    {
        private readonly IConfigService _configService;
        private readonly ICompletedDownloadService _completedDownloadService;
        private readonly IFailedDownloadService _failedDownloadService;
        private readonly ITrackedDownloadService _trackedDownloadService;
        private readonly IEventAggregator _eventAggregator;

        public DownloadProcessingService(IConfigService configService,
                                         ICompletedDownloadService completedDownloadService,
                                         IFailedDownloadService failedDownloadService,
                                         ITrackedDownloadService trackedDownloadService,
                                         IEventAggregator eventAggregator)
        {
            _configService = configService;
            _completedDownloadService = completedDownloadService;
            _failedDownloadService = failedDownloadService;
            _trackedDownloadService = trackedDownloadService;
            _eventAggregator = eventAggregator;
        }

        private void RemoveCompletedDownloads(List<TrackedDownload> trackedDownloads)
        {
            foreach (var trackedDownload in trackedDownloads.Where(c => c.DownloadItem.CanBeRemoved && c.State == TrackedDownloadState.Imported))
            {
                _eventAggregator.PublishEvent(new DownloadCompletedEvent(trackedDownload));
            }
        }

        public void Execute(ProcessMonitoredDownloadsCommand message)
        {
            var enableCompletedDownloadHandling = _configService.EnableCompletedDownloadHandling;
            var trackedDownloads = _trackedDownloadService.GetTrackedDownloads();

            foreach (var trackedDownload in trackedDownloads)
            {
                if (trackedDownload.State == TrackedDownloadState.FailedPending)
                {
                    _failedDownloadService.ProcessFailed(trackedDownload);
                }

                if (enableCompletedDownloadHandling && trackedDownload.State == TrackedDownloadState.ImportPending)
                {
                    _completedDownloadService.Import(trackedDownload);
                }
            }

            if (enableCompletedDownloadHandling && _configService.RemoveCompletedDownloads)
            {
                // Remove tracked downloads that are now complete
                RemoveCompletedDownloads(trackedDownloads);
            }

            _eventAggregator.PublishEvent(new DownloadsProcessedEvent());
        }
    }
}
