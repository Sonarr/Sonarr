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
        private readonly Logger _logger;

        public DownloadEventHub(IConfigService configService,
            IProvideDownloadClient downloadClientProvider,
            Logger logger)
        {
            _configService = configService;
            _downloadClientProvider = downloadClientProvider;
            _logger = logger;
        }

        public void Handle(DownloadCompletedEvent message)
        {
            if (!_configService.RemoveCompletedDownloads ||
                message.TrackedDownload.DownloadItem.Removed ||
                message.TrackedDownload.DownloadItem.IsReadOnly ||
                message.TrackedDownload.DownloadItem.Status == DownloadItemStatus.Downloading)
            {
                return;
            }

            RemoveFromDownloadClient(message.TrackedDownload);
        }

        public void Handle(DownloadFailedEvent message)
        {
            var trackedDownload = message.TrackedDownload;

            if (trackedDownload == null || trackedDownload.DownloadItem.IsReadOnly || _configService.RemoveFailedDownloads == false)
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
                _logger.Debug("[{0}] Removing download from {1} history", trackedDownload.DownloadItem.Title, trackedDownload.DownloadItem.DownloadClient);
                downloadClient.RemoveItem(trackedDownload.DownloadItem.DownloadId, true);
                trackedDownload.DownloadItem.Removed = true;
            }
            catch (NotSupportedException)
            {
                _logger.Warn("Removing item not supported by your download client ({0}).", downloadClient.Definition.Name);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Couldn't remove item from client {0}", trackedDownload.DownloadItem.Title);
            }
        }
    }
}