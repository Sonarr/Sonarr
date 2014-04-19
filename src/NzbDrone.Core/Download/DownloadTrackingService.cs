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
        List<TrackedDownload> GetTrackedDownloads();
        List<TrackedDownload> GetCompletedDownloads();
        List<TrackedDownload> GetQueuedDownloads();
    }

    public class DownloadTrackingService : IDownloadTrackingService, IExecute<CheckForFinishedDownloadCommand>, IHandle<ApplicationStartedEvent>
    {
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly IHistoryService _historyService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IConfigService _configService;
        private readonly IFailedDownloadService _failedDownloadService;
        private readonly ICompletedDownloadService  _completedDownloadService;
        private readonly Logger _logger;

        private readonly ICached<TrackedDownload> _trackedDownloads;
        private readonly ICached<List<TrackedDownload>> _queuedDownloads;

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

            _trackedDownloads = cacheManager.GetCache<TrackedDownload>(GetType());
            _queuedDownloads = cacheManager.GetCache<List<TrackedDownload>>(GetType(), "queued");
        }

        public List<TrackedDownload> GetTrackedDownloads()
        {
            return _trackedDownloads.Values.ToList();
        }

        public List<TrackedDownload> GetCompletedDownloads()
        {
            return _trackedDownloads.Values.Where(v => v.State == TrackedDownloadState.Downloading && v.DownloadItem.Status == DownloadItemStatus.Completed).ToList();
        }

        public List<TrackedDownload> GetQueuedDownloads()
        {
            return _queuedDownloads.Get("queued", () =>
                {
                    UpdateTrackedDownloads();

                    var enabledFailedDownloadHandling = _configService.EnableFailedDownloadHandling;
                    var enabledCompletedDownloadHandling = _configService.EnableCompletedDownloadHandling;

                    return _trackedDownloads.Values
                            .Where(v => v.State == TrackedDownloadState.Downloading)
                            .Where(v => 
                                v.DownloadItem.Status == DownloadItemStatus.Queued ||
                                v.DownloadItem.Status == DownloadItemStatus.Paused ||
                                v.DownloadItem.Status == DownloadItemStatus.Downloading ||
                                v.DownloadItem.Status == DownloadItemStatus.Failed && enabledFailedDownloadHandling ||
                                v.DownloadItem.Status == DownloadItemStatus.Completed && enabledCompletedDownloadHandling)
                            .ToList();

                }, TimeSpan.FromSeconds(5.0));
        }

        private TrackedDownload GetTrackedDownload(IDownloadClient downloadClient, DownloadClientItem queueItem)
        {
            var id = String.Format("{0}-{1}", downloadClient.Definition.Id, queueItem.DownloadClientId);
            var trackedDownload = _trackedDownloads.Get(id, () => new TrackedDownload
            {
                TrackingId = id,
                DownloadClient = downloadClient.Definition.Id,
                StartedTracking = DateTime.UtcNow,
                State = TrackedDownloadState.Unknown
            });

            trackedDownload.DownloadItem = queueItem;

            return trackedDownload;
        }

        private List<History.History> GetHistoryItems(List<History.History> grabbedHistory, string downloadClientId)
        {
            return grabbedHistory.Where(h => downloadClientId.Equals(h.Data.GetValueOrDefault(DOWNLOAD_CLIENT_ID)))
                                 .ToList();
        }


        private Boolean UpdateTrackedDownloads()
        {
            var downloadClients = _downloadClientProvider.GetDownloadClients();

            var oldTrackedDownloads = new HashSet<TrackedDownload>(_trackedDownloads.Values);
            var newTrackedDownloads = new HashSet<TrackedDownload>();

            var stateChanged = false;

            foreach (var downloadClient in downloadClients)
            {
                var downloadClientHistory = downloadClient.GetItems().Select(v => GetTrackedDownload(downloadClient, v)).ToList();
                foreach (var trackedDownload in downloadClientHistory)
                {
                    if (!oldTrackedDownloads.Contains(trackedDownload))
                    {
                        _logger.Trace("Started tracking download from history: {0}", trackedDownload.TrackingId);
                        stateChanged = true;
                    }

                    newTrackedDownloads.Add(trackedDownload);
                }
            }

            foreach (var item in oldTrackedDownloads.Except(newTrackedDownloads))
            {
                if (item.State != TrackedDownloadState.Removed)
                {
                    item.State = TrackedDownloadState.Removed;
                    stateChanged = true;

                    _logger.Debug("Item removed from download client by user: {0}", item.TrackingId);
                }
            }

            foreach (var item in newTrackedDownloads.Union(oldTrackedDownloads).Where(v => v.State == TrackedDownloadState.Removed))
            {
                _trackedDownloads.Remove(item.TrackingId);

                _logger.Trace("Stopped tracking download: {0}", item.TrackingId);
            }

            _queuedDownloads.Clear();

            return stateChanged;
        }

        private void ProcessTrackedDownloads()
        {
            var grabbedHistory = _historyService.Grabbed();
            var failedHistory = _historyService.Failed();
            var importedHistory = _historyService.Imported();

            var stateChanged = UpdateTrackedDownloads();
            
            var downloadClients = _downloadClientProvider.GetDownloadClients();
            var trackedDownloads = _trackedDownloads.Values.ToArray();

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

            if (stateChanged)
            {
                _eventAggregator.PublishEvent(new UpdateQueueEvent());
            }
        }

        public void Execute(CheckForFinishedDownloadCommand message)
        {
            ProcessTrackedDownloads();
        }

        public void Handle(ApplicationStartedEvent message)
        {
            ProcessTrackedDownloads();
        }
    }
}
