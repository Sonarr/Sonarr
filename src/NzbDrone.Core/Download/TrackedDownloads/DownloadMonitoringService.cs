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
                                             IHandleAsync<EpisodeGrabbedEvent>,
                                             IHandleAsync<EpisodeImportedEvent>
    {
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly IConfigService _configService;
        private readonly IFailedDownloadService _failedDownloadService;
        private readonly ICompletedDownloadService _completedDownloadService;
        private readonly ITrackedDownloadService _trackedDownloadService;
        private readonly Logger _logger;
        private readonly Debouncer _refreshDebounce;

        public DownloadMonitoringService(IProvideDownloadClient downloadClientProvider,
                                     IEventAggregator eventAggregator,
                                     IConfigService configService,
                                     IFailedDownloadService failedDownloadService,
                                     ICompletedDownloadService completedDownloadService,
                                     ITrackedDownloadService trackedDownloadService,
                                     Logger logger)
        {
            _downloadClientProvider = downloadClientProvider;
            _eventAggregator = eventAggregator;
            _configService = configService;
            _failedDownloadService = failedDownloadService;
            _completedDownloadService = completedDownloadService;
            _trackedDownloadService = trackedDownloadService;
            _logger = logger;

            _refreshDebounce = new Debouncer(Refresh, TimeSpan.FromSeconds(5));
        }

        private void Refresh()
        {
            var downloadClients = _downloadClientProvider.GetDownloadClients();

            var trackedDownload = new List<TrackedDownload>();

            foreach (var downloadClient in downloadClients)
            {
                var clientTrackedDownloads = ProcessClientDownloads(downloadClient);
                trackedDownload.AddRange(clientTrackedDownloads.Where(c => c.State == TrackedDownloadStage.Downloading));
            }

            _eventAggregator.PublishEvent(new TrackedDownloadRefreshedEvent(trackedDownload));
        }

        private List<TrackedDownload> ProcessClientDownloads(IDownloadClient downloadClient)
        {
            List<DownloadClientItem> downloadClientHistory = new List<DownloadClientItem>();
            var trackedDownloads = new List<TrackedDownload>();

            try
            {
                downloadClientHistory = downloadClient.GetItems().ToList();
            }
            catch (Exception ex)
            {
                _logger.WarnException("Unable to retrieve queue and history items from " + downloadClient.Definition.Name, ex);
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
            foreach (var trackedDownload in trackedDownloads.Where(c => !c.DownloadItem.IsReadOnly && c.State == TrackedDownloadStage.Imported))
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
                _logger.ErrorException("Couldn't process tracked download " + downloadItem.Title, e);
            }

            return trackedDownloads;
        }

        public void Execute(CheckForFinishedDownloadCommand message)
        {
            Refresh();
        }

        public void HandleAsync(EpisodeGrabbedEvent message)
        {
            _refreshDebounce.Execute();
        }

        public void HandleAsync(EpisodeImportedEvent message)
        {
            _refreshDebounce.Execute();
        }
    }
}
