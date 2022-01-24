using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.History
{
    public interface IHistoryService
    {
        PagingSpec<EpisodeHistory> Paged(PagingSpec<EpisodeHistory> pagingSpec);
        EpisodeHistory MostRecentForEpisode(int episodeId);
        List<EpisodeHistory> FindByEpisodeId(int episodeId);
        EpisodeHistory MostRecentForDownloadId(string downloadId);
        EpisodeHistory Get(int historyId);
        List<EpisodeHistory> GetBySeries(int seriesId, EpisodeHistoryEventType? eventType);
        List<EpisodeHistory> GetBySeason(int seriesId, int seasonNumber, EpisodeHistoryEventType? eventType);
        List<EpisodeHistory> Find(string downloadId, EpisodeHistoryEventType eventType);
        List<EpisodeHistory> FindByDownloadId(string downloadId);
        string FindDownloadId(EpisodeImportedEvent trackedDownload);
        List<EpisodeHistory> Since(DateTime date, EpisodeHistoryEventType? eventType);
    }

    public class HistoryService : IHistoryService,
                                  IHandle<EpisodeGrabbedEvent>,
                                  IHandle<EpisodeImportedEvent>,
                                  IHandle<DownloadFailedEvent>,
                                  IHandle<EpisodeFileDeletedEvent>,
                                  IHandle<EpisodeFileRenamedEvent>,
                                  IHandle<SeriesDeletedEvent>,
                                  IHandle<DownloadIgnoredEvent>
    {
        private readonly IHistoryRepository _historyRepository;
        private readonly Logger _logger;

        public HistoryService(IHistoryRepository historyRepository, Logger logger)
        {
            _historyRepository = historyRepository;
            _logger = logger;
        }

        public PagingSpec<EpisodeHistory> Paged(PagingSpec<EpisodeHistory> pagingSpec)
        {
            return _historyRepository.GetPaged(pagingSpec);
        }

        public EpisodeHistory MostRecentForEpisode(int episodeId)
        {
            return _historyRepository.MostRecentForEpisode(episodeId);
        }

        public List<EpisodeHistory> FindByEpisodeId(int episodeId)
        {
            return _historyRepository.FindByEpisodeId(episodeId);
        }

        public EpisodeHistory MostRecentForDownloadId(string downloadId)
        {
            return _historyRepository.MostRecentForDownloadId(downloadId);
        }

        public EpisodeHistory Get(int historyId)
        {
            return _historyRepository.Get(historyId);
        }

        public List<EpisodeHistory> GetBySeries(int seriesId, EpisodeHistoryEventType? eventType)
        {
            return _historyRepository.GetBySeries(seriesId, eventType);
        }

        public List<EpisodeHistory> GetBySeason(int seriesId, int seasonNumber, EpisodeHistoryEventType? eventType)
        {
            return _historyRepository.GetBySeason(seriesId, seasonNumber, eventType);
        }

        public List<EpisodeHistory> Find(string downloadId, EpisodeHistoryEventType eventType)
        {
            return _historyRepository.FindByDownloadId(downloadId).Where(c => c.EventType == eventType).ToList();
        }

        public List<EpisodeHistory> FindByDownloadId(string downloadId)
        {
            return _historyRepository.FindByDownloadId(downloadId);
        }

        public string FindDownloadId(EpisodeImportedEvent trackedDownload)
        {
            _logger.Debug("Trying to find downloadId for {0} from history", trackedDownload.ImportedEpisode.Path);

            var episodeIds = trackedDownload.EpisodeInfo.Episodes.Select(c => c.Id).ToList();
            var allHistory = _historyRepository.FindDownloadHistory(trackedDownload.EpisodeInfo.Series.Id, trackedDownload.ImportedEpisode.Quality);

            //Find download related items for these episodes
            var episodesHistory = allHistory.Where(h => episodeIds.Contains(h.EpisodeId)).ToList();

            var processedDownloadId = episodesHistory
                .Where(c => c.EventType != EpisodeHistoryEventType.Grabbed && c.DownloadId != null)
                .Select(c => c.DownloadId);

            var stillDownloading = episodesHistory.Where(c => c.EventType == EpisodeHistoryEventType.Grabbed && !processedDownloadId.Contains(c.DownloadId)).ToList();

            string downloadId = null;

            if (stillDownloading.Any())
            {
                foreach (var matchingHistory in trackedDownload.EpisodeInfo.Episodes.Select(e => stillDownloading.Where(c => c.EpisodeId == e.Id).ToList()))
                {
                    if (matchingHistory.Count != 1)
                    {
                        return null;
                    }

                    var newDownloadId = matchingHistory.Single().DownloadId;

                    if (downloadId == null || downloadId == newDownloadId)
                    {
                        downloadId = newDownloadId;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return downloadId;
        }

        public void Handle(EpisodeGrabbedEvent message)
        {
            foreach (var episode in message.Episode.Episodes)
            {
                var history = new EpisodeHistory
                {
                    EventType = EpisodeHistoryEventType.Grabbed,
                    Date = DateTime.UtcNow,
                    Quality = message.Episode.ParsedEpisodeInfo.Quality,
                    SourceTitle = message.Episode.Release.Title,
                    SeriesId = episode.SeriesId,
                    EpisodeId = episode.Id,
                    DownloadId = message.DownloadId,
                    Language = message.Episode.ParsedEpisodeInfo.Language,
                };

                history.Data.Add("Indexer", message.Episode.Release.Indexer);
                history.Data.Add("NzbInfoUrl", message.Episode.Release.InfoUrl);
                history.Data.Add("ReleaseGroup", message.Episode.ParsedEpisodeInfo.ReleaseGroup);
                history.Data.Add("Age", message.Episode.Release.Age.ToString());
                history.Data.Add("AgeHours", message.Episode.Release.AgeHours.ToString());
                history.Data.Add("AgeMinutes", message.Episode.Release.AgeMinutes.ToString());
                history.Data.Add("PublishedDate", message.Episode.Release.PublishDate.ToString("s") + "Z");
                history.Data.Add("DownloadClient", message.DownloadClient);
                history.Data.Add("DownloadClientName", message.DownloadClientName);
                history.Data.Add("Size", message.Episode.Release.Size.ToString());
                history.Data.Add("DownloadUrl", message.Episode.Release.DownloadUrl);
                history.Data.Add("Guid", message.Episode.Release.Guid);
                history.Data.Add("TvdbId", message.Episode.Release.TvdbId.ToString());
                history.Data.Add("TvRageId", message.Episode.Release.TvRageId.ToString());
                history.Data.Add("Protocol", ((int)message.Episode.Release.DownloadProtocol).ToString());
                history.Data.Add("CustomFormatScore", message.Episode.CustomFormatScore.ToString());
                history.Data.Add("SeriesMatchType", message.Episode.SeriesMatchType.ToString());

                if (!message.Episode.ParsedEpisodeInfo.ReleaseHash.IsNullOrWhiteSpace())
                {
                    history.Data.Add("ReleaseHash", message.Episode.ParsedEpisodeInfo.ReleaseHash);
                }

                var torrentRelease = message.Episode.Release as TorrentInfo;

                if (torrentRelease != null)
                {
                    history.Data.Add("TorrentInfoHash", torrentRelease.InfoHash);
                }

                _historyRepository.Insert(history);
            }
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
                downloadId = FindDownloadId(message);
            }

            foreach (var episode in message.EpisodeInfo.Episodes)
            {
                var history = new EpisodeHistory
                    {
                        EventType = EpisodeHistoryEventType.DownloadFolderImported,
                        Date = DateTime.UtcNow,
                        Quality = message.EpisodeInfo.Quality,
                        SourceTitle = message.ImportedEpisode.SceneName ?? Path.GetFileNameWithoutExtension(message.EpisodeInfo.Path),
                        SeriesId = message.ImportedEpisode.SeriesId,
                        EpisodeId = episode.Id,
                        DownloadId = downloadId,
                        Language = message.EpisodeInfo.Language
                    };

                history.Data.Add("FileId", message.ImportedEpisode.Id.ToString());
                history.Data.Add("DroppedPath", message.EpisodeInfo.Path);
                history.Data.Add("ImportedPath", Path.Combine(message.EpisodeInfo.Series.Path, message.ImportedEpisode.RelativePath));
                history.Data.Add("DownloadClient", message.DownloadClientInfo?.Type);
                history.Data.Add("DownloadClientName", message.DownloadClientInfo?.Name);
                history.Data.Add("ReleaseGroup", message.EpisodeInfo.ReleaseGroup);

                _historyRepository.Insert(history);
            }
        }

        public void Handle(DownloadFailedEvent message)
        {
            foreach (var episodeId in message.EpisodeIds)
            {
                var history = new EpisodeHistory
                {
                    EventType = EpisodeHistoryEventType.DownloadFailed,
                    Date = DateTime.UtcNow,
                    Quality = message.Quality,
                    SourceTitle = message.SourceTitle,
                    SeriesId = message.SeriesId,
                    EpisodeId = episodeId,
                    DownloadId = message.DownloadId,
                    Language = message.Language
                };

                history.Data.Add("DownloadClient", message.DownloadClient);
                history.Data.Add("DownloadClientName", message.TrackedDownload?.DownloadItem.DownloadClientInfo.Name);
                history.Data.Add("Message", message.Message);
                history.Data.Add("ReleaseGroup", message.TrackedDownload?.RemoteEpisode?.ParsedEpisodeInfo?.ReleaseGroup);

                _historyRepository.Insert(history);
            }
        }

        public void Handle(EpisodeFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.NoLinkedEpisodes)
            {
                _logger.Debug("Removing episode file from DB as part of cleanup routine, not creating history event.");
                return;
            }
            else if (message.Reason == DeleteMediaFileReason.ManualOverride)
            {
                _logger.Debug("Removing episode file from DB as part of manual override of existing file, not creating history event.");
                return;
            }

            foreach (var episode in message.EpisodeFile.Episodes.Value)
            {
                var history = new EpisodeHistory
                {
                    EventType = EpisodeHistoryEventType.EpisodeFileDeleted,
                    Date = DateTime.UtcNow,
                    Quality = message.EpisodeFile.Quality,
                    SourceTitle = message.EpisodeFile.Path,
                    SeriesId = message.EpisodeFile.SeriesId,
                    EpisodeId = episode.Id,
                    Language = message.EpisodeFile.Language
                };

                history.Data.Add("Reason", message.Reason.ToString());
                history.Data.Add("ReleaseGroup", message.EpisodeFile.ReleaseGroup);

                _historyRepository.Insert(history);
            }
        }

        public void Handle(EpisodeFileRenamedEvent message)
        {
            var sourcePath = message.OriginalPath;
            var sourceRelativePath = message.Series.Path.GetRelativePath(message.OriginalPath);
            var path = Path.Combine(message.Series.Path, message.EpisodeFile.RelativePath);
            var relativePath = message.EpisodeFile.RelativePath;

            foreach (var episode in message.EpisodeFile.Episodes.Value)
            {
                var history = new EpisodeHistory
                {
                    EventType = EpisodeHistoryEventType.EpisodeFileRenamed,
                    Date = DateTime.UtcNow,
                    Quality = message.EpisodeFile.Quality,
                    SourceTitle = message.OriginalPath,
                    SeriesId = message.EpisodeFile.SeriesId,
                    EpisodeId = episode.Id,
                    Language = message.EpisodeFile.Language
                };

                history.Data.Add("SourcePath", sourcePath);
                history.Data.Add("SourceRelativePath", sourceRelativePath);
                history.Data.Add("Path", path);
                history.Data.Add("RelativePath", relativePath);
                history.Data.Add("ReleaseGroup", message.EpisodeFile.ReleaseGroup);

                _historyRepository.Insert(history);
            }
        }

        public void Handle(DownloadIgnoredEvent message)
        {
            var historyToAdd = new List<EpisodeHistory>();

            foreach (var episodeId in message.EpisodeIds)
            {
                var history = new EpisodeHistory
                              {
                                  EventType = EpisodeHistoryEventType.DownloadIgnored,
                                  Date = DateTime.UtcNow,
                                  Quality = message.Quality,
                                  SourceTitle = message.SourceTitle,
                                  SeriesId = message.SeriesId,
                                  EpisodeId = episodeId,
                                  DownloadId = message.DownloadId,
                                  Language = message.Language
                              };

                history.Data.Add("DownloadClient", message.DownloadClientInfo.Type);
                history.Data.Add("DownloadClientName", message.DownloadClientInfo.Name);
                history.Data.Add("Message", message.Message);
                history.Data.Add("ReleaseGroup", message.TrackedDownload?.RemoteEpisode?.ParsedEpisodeInfo?.ReleaseGroup);

                historyToAdd.Add(history);
            }

            _historyRepository.InsertMany(historyToAdd);
        }

        public void Handle(SeriesDeletedEvent message)
        {
            _historyRepository.DeleteForSeries(message.Series.Id);
        }

        public List<EpisodeHistory> Since(DateTime date, EpisodeHistoryEventType? eventType)
        {
            return _historyRepository.Since(date, eventType);
        }
    }
}
