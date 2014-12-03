using System.Linq;
using Nancy;
using Nancy.Responses;
using NzbDrone.Api.Extensions;
using NzbDrone.Api.REST;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Queue;

namespace NzbDrone.Api.Queue
{
    public class QueueActionModule : NzbDroneRestModule<QueueResource>
    {
        private readonly IQueueService _queueService;
        private readonly IDownloadTrackingService _downloadTrackingService;
        private readonly ICompletedDownloadService _completedDownloadService;
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly IPendingReleaseService _pendingReleaseService;
        private readonly IDownloadService _downloadService;

        public QueueActionModule(IQueueService queueService,
                                 IDownloadTrackingService downloadTrackingService,
                                 ICompletedDownloadService completedDownloadService,
                                 IProvideDownloadClient downloadClientProvider,
                                 IPendingReleaseService pendingReleaseService,
                                 IDownloadService downloadService)
        {
            _queueService = queueService;
            _downloadTrackingService = downloadTrackingService;
            _completedDownloadService = completedDownloadService;
            _downloadClientProvider = downloadClientProvider;
            _pendingReleaseService = pendingReleaseService;
            _downloadService = downloadService;

            Delete[@"/(?<id>[\d]{1,10})"] = x => Remove((int)x.Id);
            Post["/import"] = x => Import();
            Post["/grab"] = x => Grab();
        }

        private Response Remove(int id)
        {
            var pendingRelease = _pendingReleaseService.FindPendingQueueItem(id);

            if (pendingRelease != null)
            {
                _pendingReleaseService.RemovePendingQueueItem(id);
            }

            var trackedDownload = GetTrackedDownload(id);

            if (trackedDownload == null)
            {
                throw new NotFoundException();
            }

            var downloadClient = _downloadClientProvider.Get(trackedDownload.DownloadClient);

            if (downloadClient == null)
            {
                throw new BadRequestException();
            }

            downloadClient.RemoveItem(trackedDownload.DownloadItem.DownloadClientId);

            return new object().AsResponse();
        }

        private JsonResponse<QueueResource> Import()
        {
            var resource = Request.Body.FromJson<QueueResource>();
            var trackedDownload = GetTrackedDownload(resource.Id);
                
            _completedDownloadService.Import(trackedDownload);

            return resource.AsResponse();
        }

        private JsonResponse<QueueResource> Grab()
        {
            var resource = Request.Body.FromJson<QueueResource>();

            var pendingRelease = _pendingReleaseService.FindPendingQueueItem(resource.Id);

            if (pendingRelease == null)
            {
                throw new NotFoundException();
            }

            _downloadService.DownloadReport(pendingRelease.RemoteEpisode);

            return resource.AsResponse();
        }

        private TrackedDownload GetTrackedDownload(int queueId)
        {
            var queueItem = _queueService.Find(queueId);

            if (queueItem == null)
            {
                throw new NotFoundException();
            }

            var trackedDownload = _downloadTrackingService.Find(queueItem.TrackingId);

            if (trackedDownload == null)
            {
                throw new NotFoundException();
            }

            return trackedDownload;
        }
    }
}
