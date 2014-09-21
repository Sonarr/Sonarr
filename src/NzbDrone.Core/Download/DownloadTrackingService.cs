using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Cache;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.History;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Queue;

namespace NzbDrone.Core.Download
{
    public interface IDownloadTrackingService
    {
        TrackedDownload[] GetTrackedDownloads();
        TrackedDownload[] GetCompletedDownloads();
        TrackedDownload[] GetQueuedDownloads();

        void MarkAsFailed(Int32 historyId);
    }

    public class DownloadTrackingService : IDownloadTrackingService, IExecute<CheckForFinishedDownloadCommand>, IHandleAsync<ApplicationStartedEvent>, IHandle<EpisodeGrabbedEvent>
    {
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly IHistoryService _historyService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IConfigService _configService;
        private readonly IFailedDownloadService _failedDownloadService;
        private readonly ICompletedDownloadService  _completedDownloadService;
        private readonly Logger _logger;

        private readonly ICached<TrackedDownload[]> _trackedDownloadCache;

        public static string DOWNLOAD_CLIENT = "downloadClient";
        public static string DOWNLOAD_CLIENT_ID = "downloadClientId";

        public DownloadTrackingService(IProvideDownloadClient downloadClientProvider,
                                     IHistoryService historyService,
                                     IEventAggregator eventAggregator,
                                     IConfigService configService,
                                     ICacheManager cacheManager,
                                     IFailedDownloadService failedDownloadService,
                                     ICompletedDownloadService completedDownloadService,
                                     Logger logger)
        {
            _downloadClientProvider = downloadClientProvider;
            _historyService = historyService;
            _eventAggregator = eventAggregator;
            _configService = configService;
            _failedDownloadService = failedDownloadService;
            _completedDownloadService = completedDownloadService;
            _logger = logger;

            _trackedDownloadCache = cacheManager.GetCache<TrackedDownload[]>(GetType());
        }

        public TrackedDownload[] GetTrackedDownloads()
        {
            return _trackedDownloadCache.Get("tracked", () => new TrackedDownload[0]);
        }

        public TrackedDownload[] GetCompletedDownloads()
        {
            return GetTrackedDownloads()
                    .Where(v => v.State == TrackedDownloadState.Downloading && v.DownloadItem.Status == DownloadItemStatus.Completed)
                    .ToArray();
        }

        public TrackedDownload[] GetQueuedDownloads()
        {
            return _trackedDownloadCache.Get("queued", () =>
                {
                    UpdateTrackedDownloads();

                    return FilterQueuedDownloads(GetTrackedDownloads());

                }, TimeSpan.FromSeconds(5.0));
        }

        public void MarkAsFailed(Int32 historyId)
        {
            var item = _historyService.Get(historyId);
            
            var trackedDownload = GetTrackedDownloads()
                .Where(h => h.DownloadItem.DownloadClientId.Equals(item.Data.GetValueOrDefault(DOWNLOAD_CLIENT_ID)))
                .FirstOrDefault();

            if (trackedDownload != null && trackedDownload.State == TrackedDownloadState.Unknown)
            {
                ProcessTrackedDownloads();
            }

            _failedDownloadService.MarkAsFailed(trackedDownload, item);
        }

        private TrackedDownload[] FilterQueuedDownloads(IEnumerable<TrackedDownload> trackedDownloads)
        {
            var enabledFailedDownloadHandling = _configService.EnableFailedDownloadHandling;
            var enabledCompletedDownloadHandling = _configService.EnableCompletedDownloadHandling;

            return trackedDownloads
                    .Where(v => v.State == TrackedDownloadState.Downloading)
                    .Where(v => 
                        v.DownloadItem.Status == DownloadItemStatus.Queued ||
                        v.DownloadItem.Status == DownloadItemStatus.Paused ||
                        v.DownloadItem.Status == DownloadItemStatus.Downloading ||
                        v.DownloadItem.Status == DownloadItemStatus.Warning ||
                        v.DownloadItem.Status == DownloadItemStatus.Failed && enabledFailedDownloadHandling ||
                        v.DownloadItem.Status == DownloadItemStatus.Completed && enabledCompletedDownloadHandling)
                    .ToArray();
        }
        
        private List<History.History> GetHistoryItems(List<History.History> grabbedHistory, string downloadClientId)
        {
            return grabbedHistory.Where(h => downloadClientId.Equals(h.Data.GetValueOrDefault(DOWNLOAD_CLIENT_ID)))
                                 .ToList();
        }

        private Boolean UpdateTrackedDownloads()
        {
            var downloadClients = _downloadClientProvider.GetDownloadClients();

            var oldTrackedDownloads = GetTrackedDownloads().ToDictionary(v => v.TrackingId);
            var newTrackedDownloads = new Dictionary<String, TrackedDownload>();

            var stateChanged = false;

            foreach (var downloadClient in downloadClients)
            {
                List<DownloadClientItem> downloadClientHistory;
                try
                {
                    downloadClientHistory = downloadClient.GetItems().ToList();
                }
                catch (Exception ex)
                {
                    _logger.WarnException("Unable to retrieve queue and history items from " + downloadClient.Definition.Name, ex);
                    continue;
                }
                foreach (var downloadItem in downloadClientHistory)
                {
                    var trackingId = String.Format("{0}-{1}", downloadClient.Definition.Id, downloadItem.DownloadClientId);
                    TrackedDownload trackedDownload;

                    if (newTrackedDownloads.ContainsKey(trackingId)) continue;

                    if (!oldTrackedDownloads.TryGetValue(trackingId, out trackedDownload))
                    {
                        trackedDownload = new TrackedDownload
                        {
                            TrackingId = trackingId,
                            DownloadClient = downloadClient.Definition.Id,
                            StartedTracking = DateTime.UtcNow,
                            State = TrackedDownloadState.Unknown
                        };

                        _logger.Debug("[{0}] Started tracking download with id {1}.", downloadItem.Title, trackingId);
                        stateChanged = true;
                    }

                    trackedDownload.DownloadItem = downloadItem;

                    newTrackedDownloads[trackingId] = trackedDownload;
                }
            }

            foreach (var trackedDownload in oldTrackedDownloads.Values.Where(v => !newTrackedDownloads.ContainsKey(v.TrackingId)))
            {
                if (trackedDownload.State != TrackedDownloadState.Removed)
                {
                    trackedDownload.State = TrackedDownloadState.Removed;
                    stateChanged = true;

                    _logger.Debug("[{0}] Item with id {1} removed from download client directly (possibly by user).", trackedDownload.DownloadItem.Title, trackedDownload.TrackingId);
                }

                _logger.Debug("[{0}] Stopped tracking download with id {1}.", trackedDownload.DownloadItem.Title, trackedDownload.TrackingId);
            }

            _trackedDownloadCache.Set("tracked", newTrackedDownloads.Values.ToArray());

            return stateChanged;
        }

        private void ProcessTrackedDownloads()
        {
            var grabbedHistory = _historyService.Grabbed();
            var failedHistory = _historyService.Failed();
            var importedHistory = _historyService.Imported();

            var stateChanged = UpdateTrackedDownloads();
            
            var downloadClients = _downloadClientProvider.GetDownloadClients();
            var trackedDownloads = GetTrackedDownloads();

            foreach (var trackedDownload in trackedDownloads)
            {
                var downloadClient = downloadClients.Single(v => v.Definition.Id == trackedDownload.DownloadClient);

                var state = trackedDownload.State;

                if (trackedDownload.State == TrackedDownloadState.Unknown)
                {
                    trackedDownload.State = TrackedDownloadState.Downloading;
                }

                _failedDownloadService.CheckForFailedItem(downloadClient, trackedDownload, grabbedHistory, failedHistory);
                _completedDownloadService.CheckForCompletedItem(downloadClient, trackedDownload, grabbedHistory, importedHistory);

                if (state != trackedDownload.State)
                {
                    stateChanged = true;
                }
            }

            _trackedDownloadCache.Set("queued", FilterQueuedDownloads(trackedDownloads), TimeSpan.FromSeconds(5.0));

            if (stateChanged)
            {
                _eventAggregator.PublishEvent(new UpdateQueueEvent());
            }
        }

        public void Execute(CheckForFinishedDownloadCommand message)
        {
            ProcessTrackedDownloads();
        }

        public void HandleAsync(ApplicationStartedEvent message)
        {
            ProcessTrackedDownloads();
        }

        public void Handle(EpisodeGrabbedEvent message)
        {
            ProcessTrackedDownloads();
        }
    }
}
