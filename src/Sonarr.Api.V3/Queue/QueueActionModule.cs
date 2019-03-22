using System;
using System.Collections.Generic;
using Nancy;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Queue;
using Sonarr.Http;
using Sonarr.Http.Extensions;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Queue
{
    public class QueueActionModule : SonarrRestModule<QueueResource>
    {
        private readonly IQueueService _queueService;
        private readonly ITrackedDownloadService _trackedDownloadService;
        private readonly IFailedDownloadService _failedDownloadService;
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly IPendingReleaseService _pendingReleaseService;
        private readonly IDownloadService _downloadService;

        public QueueActionModule(IQueueService queueService,
                                 ITrackedDownloadService trackedDownloadService,
                                 IFailedDownloadService failedDownloadService,
                                 IProvideDownloadClient downloadClientProvider,
                                 IPendingReleaseService pendingReleaseService,
                                 IDownloadService downloadService)
        {
            _queueService = queueService;
            _trackedDownloadService = trackedDownloadService;
            _failedDownloadService = failedDownloadService;
            _downloadClientProvider = downloadClientProvider;
            _pendingReleaseService = pendingReleaseService;
            _downloadService = downloadService;

            Post[@"/grab/(?<id>[\d]{1,10})"] = x => Grab((int)x.Id);
            Post["/grab/bulk"] = x => Grab();

            Delete[@"/(?<id>[\d]{1,10})"] = x => Remove((int)x.Id);
            Delete["/bulk"] = x => Remove();
        }

        private Response Grab(int id)
        {
            var pendingRelease = _pendingReleaseService.FindPendingQueueItem(id);

            if (pendingRelease == null)
            {
                throw new NotFoundException();
            }

            _downloadService.DownloadReport(pendingRelease.RemoteEpisode);

            return new object().AsResponse();
        }

        private Response Grab()
        {
            var resource = Request.Body.FromJson<QueueBulkResource>();

            foreach (var id in resource.Ids)
            {
                var pendingRelease = _pendingReleaseService.FindPendingQueueItem(id);

                if (pendingRelease == null)
                {
                    throw new NotFoundException();
                }

                _downloadService.DownloadReport(pendingRelease.RemoteEpisode);
            }

            return new object().AsResponse();
        }

        private Response Remove(int id)
        {
            var blacklist = Request.GetBooleanQueryParameter("blacklist");

            var trackedDownload = Remove(id, blacklist);

            if (trackedDownload != null)
            {
                _trackedDownloadService.StopTracking(trackedDownload.DownloadItem.DownloadId);
            }

            return new object().AsResponse();
        }

        private Response Remove()
        {
            var blacklist = Request.GetBooleanQueryParameter("blacklist");

            var resource = Request.Body.FromJson<QueueBulkResource>();
            var trackedDownloadIds = new List<string>();

            foreach (var id in resource.Ids)
            {
                var trackedDownload = Remove(id, blacklist);

                if (trackedDownload != null)
                {
                    trackedDownloadIds.Add(trackedDownload.DownloadItem.DownloadId);
                }
            }

            _trackedDownloadService.StopTracking(trackedDownloadIds);

            return new object().AsResponse();
        }

        private TrackedDownload Remove(int id, bool blacklist)
        {
            var pendingRelease = _pendingReleaseService.FindPendingQueueItem(id);

            if (pendingRelease != null)
            {
                _pendingReleaseService.RemovePendingQueueItems(pendingRelease.Id);

                return null;
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

            downloadClient.RemoveItem(trackedDownload.DownloadItem.DownloadId, true);

            if (blacklist)
            {
                _failedDownloadService.MarkAsFailed(trackedDownload.DownloadItem.DownloadId);
            }

            return trackedDownload;
        }

        private TrackedDownload GetTrackedDownload(int queueId)
        {
            var queueItem = _queueService.Find(queueId);

            if (queueItem == null)
            {
                throw new NotFoundException();
            }

            var trackedDownload = _trackedDownloadService.Find(queueItem.DownloadId);

            if (trackedDownload == null)
            {
                throw new NotFoundException();
            }

            return trackedDownload;
        }
    }
}
