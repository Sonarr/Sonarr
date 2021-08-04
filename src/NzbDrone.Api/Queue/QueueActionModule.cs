﻿using System;
using Nancy;
using Sonarr.Http.Extensions;
using Sonarr.Http.REST;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Queue;
using Sonarr.Http;

namespace NzbDrone.Api.Queue
{
    public class QueueActionModule : SonarrRestModule<QueueResource>
    {
        private readonly IQueueService _queueService;
        private readonly ITrackedDownloadService _trackedDownloadService;
        private readonly ICompletedDownloadService _completedDownloadService;
        private readonly IFailedDownloadService _failedDownloadService;
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly IPendingReleaseService _pendingReleaseService;
        private readonly IDownloadService _downloadService;

        public QueueActionModule(IQueueService queueService,
                                 ITrackedDownloadService trackedDownloadService,
                                 ICompletedDownloadService completedDownloadService,
                                 IFailedDownloadService failedDownloadService,
                                 IProvideDownloadClient downloadClientProvider,
                                 IPendingReleaseService pendingReleaseService,
                                 IDownloadService downloadService)
        {
            _queueService = queueService;
            _trackedDownloadService = trackedDownloadService;
            _completedDownloadService = completedDownloadService;
            _failedDownloadService = failedDownloadService;
            _downloadClientProvider = downloadClientProvider;
            _pendingReleaseService = pendingReleaseService;
            _downloadService = downloadService;

            Delete(@"/(?<id>[\d]{1,10})",  x => Remove((int)x.Id));
            Post("/import",  x => Import());
            Post("/grab",  x => Grab());
        }

        private object Remove(int id)
        {
            var blocklist = false;
            var blocklistQuery = Request.Query.blocklist;

            // blacklist maintained for backwards compatability, UI uses blocklist.
            var blacklistQuery = Request.Query.blacklist;

            if (blocklistQuery.HasValue)
            {
                blocklist = Convert.ToBoolean(blocklistQuery.Value);
            }
            else if (blacklistQuery.HasValue)
            {
                blocklist = Convert.ToBoolean(blacklistQuery.Value);
            }

            var pendingRelease = _pendingReleaseService.FindPendingQueueItem(id);

            if (pendingRelease != null)
            {
                _pendingReleaseService.RemovePendingQueueItems(pendingRelease.Id);

                return new object();
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

            downloadClient.RemoveItem(trackedDownload.DownloadItem, true);

            if (blocklist)
            {
                _failedDownloadService.MarkAsFailed(trackedDownload.DownloadItem.DownloadId);
            }

            return new object();
        }

        private object Import()
        {
            throw new BadRequestException("No longer available");
        }

        private object Grab()
        {
            var resource = Request.Body.FromJson<QueueResource>();

            var pendingRelease = _pendingReleaseService.FindPendingQueueItem(resource.Id);

            if (pendingRelease == null)
            {
                throw new NotFoundException();
            }

            _downloadService.DownloadReport(pendingRelease.RemoteEpisode);

            return resource;
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
