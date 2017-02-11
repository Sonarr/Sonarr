using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.TPL;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Download.TrackedDownloads
{
    public class DownloadMonitoringService : IExecute<CheckForFinishedDownloadCommand>,
                                             IHandle<EpisodeGrabbedEvent>,
                                             IHandle<EpisodeImportedEvent>,
                                             IHandle<TrackedDownloadsRemovedEvent>
    {
        private readonly IDownloadClientStatusService _downloadClientStatusService;
        private readonly IDownloadClientFactory _downloadClientFactory;
        private readonly IEventAggregator _eventAggregator;
        private readonly IManageCommandQueue _manageCommandQueue;
        private readonly IConfigService _configService;
        private readonly IFailedDownloadService _failedDownloadService;
        private readonly ICompletedDownloadService _completedDownloadService;
        private readonly ITrackedDownloadService _trackedDownloadService;
        private readonly Logger _logger;
        private readonly Debouncer _refreshDebounce;

        public DownloadMonitoringService(IDownloadClientStatusService downloadClientStatusService,
                                         IDownloadClientFactory downloadClientFactory,
                                         IEventAggregator eventAggregator,
                                         IManageCommandQueue manageCommandQueue,
                                         IConfigService configService,
                                         IFailedDownloadService failedDownloadService,
                                         ICompletedDownloadService completedDownloadService,
                                         ITrackedDownloadService trackedDownloadService,
                                         Logger logger)
        {
            _downloadClientStatusService = downloadClientStatusService;
            _downloadClientFactory = downloadClientFactory;
            _eventAggregator = eventAggregator;
            _manageCommandQueue = manageCommandQueue;
            _configService = configService;
            _failedDownloadService = failedDownloadService;
            _completedDownloadService = completedDownloadService;
            _trackedDownloadService = trackedDownloadService;
            _logger = logger;

            _refreshDebounce = new Debouncer(QueueRefresh, TimeSpan.FromSeconds(5));
        }

        private void QueueRefresh()
        {
            _manageCommandQueue.Push(new CheckForFinishedDownloadCommand());
        }

        private void Refresh()
        {
            _refreshDebounce.Pause();
            try
            {
                var downloadClients = _downloadClientFactory.DownloadHandlingEnabled();

                var trackedDownloads = new List<TrackedDownload>();

                foreach (var downloadClient in downloadClients)
                {
                    var clientTrackedDownloads = ProcessClientDownloads(downloadClient);

                    trackedDownloads.AddRange(clientTrackedDownloads.Where(DownloadIsTrackable));
                }

                _trackedDownloadService.UpdateTrackable(trackedDownloads);
                _eventAggregator.PublishEvent(new TrackedDownloadRefreshedEvent(trackedDownloads));
            }
            finally
            {
                _refreshDebounce.Resume();
            }
        }

        private List<TrackedDownload> ProcessClientDownloads(IDownloadClient downloadClient)
        {
            List<DownloadClientItem> downloadClientHistory = new List<DownloadClientItem>();
            var trackedDownloads = new List<TrackedDownload>();

            try
            {
                downloadClientHistory = downloadClient.GetItems().ToList();

                _downloadClientStatusService.RecordSuccess(downloadClient.Definition.Id);
            }
            catch (Exception ex)
            {
                // TODO: Stop tracking items for the offline client

                _downloadClientStatusService.RecordFailure(downloadClient.Definition.Id);
                _logger.Warn(ex, "Unable to retrieve queue and history items from " + downloadClient.Definition.Name);
            }

            foreach (var downloadItem in downloadClientHistory)
            {
                var newItems = ProcessClientItems(downloadClient, downloadItem);
                trackedDownloads.AddRange(newItems);
            }

            if (_configService.EnableCompletedDownloadHandling && _configService.RemoveCompletedDownloads)
            {
                RemoveCompletedDownloads(trackedDownloads);
            }

            return trackedDownloads;
        }

        private void RemoveCompletedDownloads(List<TrackedDownload> trackedDownloads)
        {
            foreach (var trackedDownload in trackedDownloads.Where(c => c.DownloadItem.CanBeRemoved && c.State == TrackedDownloadStage.Imported))
            {
                _eventAggregator.PublishEvent(new DownloadCompletedEvent(trackedDownload));
            }
        }

        private List<TrackedDownload> ProcessClientItems(IDownloadClient downloadClient, DownloadClientItem downloadItem)
        {
            var trackedDownloads = new List<TrackedDownload>();
            try
            {
                var trackedDownload = _trackedDownloadService.TrackDownload((DownloadClientDefinition)downloadClient.Definition, downloadItem);
                if (trackedDownload != null && trackedDownload.State == TrackedDownloadStage.Downloading)
                {
                    _failedDownloadService.Process(trackedDownload);

                    if (_configService.EnableCompletedDownloadHandling)
                    {
                        _completedDownloadService.Process(trackedDownload);
                    }
                }

                trackedDownloads.AddIfNotNull(trackedDownload);

            }
            catch (Exception e)
            {
                _logger.Error(e, "Couldn't process tracked download {0}", downloadItem.Title);
            }

            return trackedDownloads;
        }

        private bool DownloadIsTrackable(TrackedDownload trackedDownload)
        {
            // If the download has already been imported or failed don't track it
            if (trackedDownload.State != TrackedDownloadStage.Downloading)
            {
                return false;
            }

            // If CDH is disabled and the download status is complete don't track it
            if (!_configService.EnableCompletedDownloadHandling && trackedDownload.DownloadItem.Status == DownloadItemStatus.Completed)
            {
                return false;
            }

            return true;
        }

        public void Execute(CheckForFinishedDownloadCommand message)
        {
            Refresh();
        }

        public void Handle(EpisodeGrabbedEvent message)
        {
            _refreshDebounce.Execute();
        }

        public void Handle(EpisodeImportedEvent message)
        {
            _refreshDebounce.Execute();
        }

        public void Handle(TrackedDownloadsRemovedEvent message)
        {
            var trackedDownloads = _trackedDownloadService.GetTrackedDownloads().Where(t => t.IsTrackable && DownloadIsTrackable(t)).ToList();

            _eventAggregator.PublishEvent(new TrackedDownloadRefreshedEvent(trackedDownloads));
        }
    }
}
