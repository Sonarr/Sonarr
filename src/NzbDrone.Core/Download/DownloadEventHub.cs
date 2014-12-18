using System;
using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Download
{
    public class DownloadCompletedEvent : IEvent
    {
        public TrackedDownload TrackedDownload { get; private set; }

        public DownloadCompletedEvent(TrackedDownload trackedDownload)
        {
            TrackedDownload = trackedDownload;
        }
    }


    public class DownloadEventHub : IHandle<DownloadFailedEvent>,
                                    IHandle<DownloadCompletedEvent>
    {
        private readonly IConfigService _configService;
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly ITrackedDownloadService _trackedDownloadService;
        private readonly Logger _logger;

        public DownloadEventHub(IConfigService configService,
            IProvideDownloadClient downloadClientProvider,
            ITrackedDownloadService trackedDownloadService,
            Logger logger)
        {
            _configService = configService;
            _downloadClientProvider = downloadClientProvider;
            _trackedDownloadService = trackedDownloadService;
            _logger = logger;
        }

        public void Handle(DownloadCompletedEvent message)
        {
            if (message.TrackedDownload.DownloadItem.Removed || message.TrackedDownload.DownloadItem.IsReadOnly || !_configService.RemoveCompletedDownloads)
            {
                return;
            }

            RemoveFromDownloadClient(message.TrackedDownload);
        }

        public void Handle(DownloadFailedEvent message)
        {
            var trackedDownload = _trackedDownloadService.Find(message.DownloadId);


            if (trackedDownload == null || trackedDownload.DownloadItem.IsReadOnly || !_configService.RemoveFailedDownloads)
            {
                return;
            }

            RemoveFromDownloadClient(trackedDownload);
        }


        private void RemoveFromDownloadClient(TrackedDownload trackedDownload)
        {
            var downloadClient = _downloadClientProvider.Get(trackedDownload.DownloadClient);
            try
            {
                _logger.Debug("[{0}] Removing download from {1} history", trackedDownload.DownloadItem.DownloadClient);
                downloadClient.RemoveItem(trackedDownload.DownloadItem.DownloadId);
                trackedDownload.DownloadItem.Removed = true;
            }
            catch (NotSupportedException)
            {
                _logger.Warn("Removing item not supported by your download client ({0}).", downloadClient.Definition.Name);
            }
            catch (Exception e)
            {
                _logger.ErrorException("Couldn't remove item from client " + trackedDownload.DownloadItem.Title, e);
            }
        }
    }
}