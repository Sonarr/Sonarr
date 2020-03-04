using System;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Download.History
{
    public interface IDownloadHistoryService
    {
        bool DownloadAlreadyImported(string downloadId);
        DownloadHistory GetLatestDownloadHistoryItem(string downloadId);
    }

    public class DownloadHistoryService : IDownloadHistoryService,
                                          IHandle<EpisodeGrabbedEvent>,
                                          IHandle<EpisodeImportedEvent>,
                                          IHandle<DownloadCompletedEvent>,
                                          IHandle<DownloadFailedEvent>,
                                          IHandle<DownloadIgnoredEvent>,
                                          IHandle<SeriesDeletedEvent>

    {
        private readonly IDownloadHistoryRepository _repository;
        private readonly IHistoryService _historyService;

        public DownloadHistoryService(IDownloadHistoryRepository repository, IHistoryService historyService)
        {
            _repository = repository;
            _historyService = historyService;
        }

        public bool DownloadAlreadyImported(string downloadId)
        {
            var events = _repository.FindByDownloadId(downloadId);

            // Events are ordered by date descending, if a grabbed event comes before an imported event then it was never imported
            // or grabbed again after importing and should be reprocessed.
            foreach (var e in events)
            {
                if (e.EventType == DownloadHistoryEventType.DownloadGrabbed)
                {
                    return false;
                }

                if (e.EventType == DownloadHistoryEventType.DownloadImported)
                {
                    return true;
                }
            }

            return false;
        }

        public DownloadHistory GetLatestDownloadHistoryItem(string downloadId)
        {
            var events = _repository.FindByDownloadId(downloadId);

            // Events are ordered by date descending. We'll return the most recent expected event.
            foreach (var e in events)
            {
                if (e.EventType == DownloadHistoryEventType.DownloadGrabbed)
                {
                    return e;
                }

                if (e.EventType == DownloadHistoryEventType.DownloadImported)
                {
                    return e;
                }

                if (e.EventType == DownloadHistoryEventType.DownloadFailed)
                {
                    return e;
                }
            }

            return null;
        }

        public void Handle(EpisodeGrabbedEvent message)
        {
            _repository.Insert(new DownloadHistory
            {
                EventType = DownloadHistoryEventType.DownloadGrabbed,
                SeriesId = message.Episode.Series.Id,
                DownloadId = message.DownloadId,
                SourceTitle = message.Episode.Release.Title,
                Date = DateTime.UtcNow
            });
        }

        public void Handle(EpisodeImportedEvent message)
        {
            if (!message.NewDownload)
            {
                return;
            }

            var downloadId = message.DownloadId;

            if (downloadId.IsNullOrWhiteSpace())
            {
                downloadId = _historyService.FindDownloadId(message);
            }

            if (downloadId.IsNullOrWhiteSpace())
            {
                return;
            }

            _repository.Insert(new DownloadHistory
            {
                EventType = DownloadHistoryEventType.FileImported,
                SeriesId = message.EpisodeInfo.Series.Id,
                DownloadId = downloadId,
                SourceTitle = message.EpisodeInfo.Path,
                Date = DateTime.UtcNow
            });
        }

        public void Handle(DownloadCompletedEvent message)
        {
            _repository.Insert(new DownloadHistory
            {
                EventType = DownloadHistoryEventType.DownloadImported,
                SeriesId = message.TrackedDownload.RemoteEpisode.Series.Id,
                DownloadId = message.TrackedDownload.DownloadItem.DownloadId,
                SourceTitle = message.TrackedDownload.DownloadItem.OutputPath.ToString(),
                Date = DateTime.UtcNow
            });
        }

        public void Handle(DownloadFailedEvent message)
        {
            _repository.Insert(new DownloadHistory
            {
                EventType = DownloadHistoryEventType.DownloadFailed,
                SeriesId = message.TrackedDownload.RemoteEpisode.Series.Id,
                DownloadId = message.TrackedDownload.DownloadItem.DownloadId,
                SourceTitle = message.SourceTitle,
                Date = DateTime.UtcNow
            });
        }

        public void Handle(DownloadIgnoredEvent message)
        {
            _repository.Insert(new DownloadHistory
            {
                EventType = DownloadHistoryEventType.DownloadIgnored,
                SeriesId = message.SeriesId,
                DownloadId = message.DownloadId,
                SourceTitle = message.SourceTitle,
                Date = DateTime.UtcNow
            });
        }

        public void Handle(SeriesDeletedEvent message)
        {
            _repository.DeleteBySeriesId(message.Series.Id);
        }
    }
}
