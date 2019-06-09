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
            if (_configService.RemoveCompletedDownloads &&
                !message.TrackedDownload.DownloadItem.Removed &&
                message.TrackedDownload.DownloadItem.CanBeRemoved &&
                message.TrackedDownload.DownloadItem.Status != DownloadItemStatus.Downloading)
            {
                RemoveFromDownloadClient(message.TrackedDownload);
            }
            else
            {
                MarkItemAsImported(message.TrackedDownload);
            }           
        }

        public void Handle(DownloadFailedEvent message)
        {
            var trackedDownload = message.TrackedDownload;

            if (trackedDownload == null || !trackedDownload.DownloadItem.CanBeRemoved || _configService.RemoveFailedDownloads == false)
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
                _logger.Error(e, "Couldn't remove item {0} from client {1}", trackedDownload.DownloadItem.Title, downloadClient.Name);
            }
        }

        private void MarkItemAsImported(TrackedDownload trackedDownload)
        {
            var downloadClient = _downloadClientProvider.Get(trackedDownload.DownloadClient);
            try
            {
                _logger.Debug("[{0}] Marking download as imported from {1}", trackedDownload.DownloadItem.Title, trackedDownload.DownloadItem.DownloadClient);
                downloadClient.MarkItemAsImported(trackedDownload.DownloadItem);
            }
            catch (NotSupportedException e)
            {
                _logger.Debug(e.Message);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Couldn't mark item {0} as imported from client {1}", trackedDownload.DownloadItem.Title, downloadClient.Name);
            }
        }
    }
}
