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
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.History
{
    public interface IHistoryService
    {
        QualityModel GetBestEpisodeQualityInHistory(Profile profile, int episodeId);
        QualityModel GetBestMovieQualityInHistory(Profile profile, int movieId);
        PagingSpec<History> Paged(PagingSpec<History> pagingSpec);
        History MostRecentForEpisode(int episodeId);
        History MostRecentForMovie(int movieId);
        History MostRecentForDownloadId(string downloadId);
        History Get(int historyId);
        List<History> Find(string downloadId, HistoryEventType eventType);
        List<History> FindByDownloadId(string downloadId);
    }

    public class HistoryService : IHistoryService,
                                  IHandle<EpisodeGrabbedEvent>,
                                  IHandle<EpisodeImportedEvent>,
                                  IHandle<DownloadFailedEvent>,
                                  IHandle<EpisodeFileDeletedEvent>,
                                  IHandle<MovieGrabbedEvent>,
                                  IHandle<MovieImportedEvent>,
                                  IHandle<MovieFileDeletedEvent>
    {
        private readonly IHistoryRepository _historyRepository;
        private readonly Logger _logger;

        public HistoryService(IHistoryRepository historyRepository, Logger logger)
        {
            _historyRepository = historyRepository;
            _logger = logger;
        }


        public void Handle(DownloadFailedEvent message)
        {
            foreach (var episodeId in message.EpisodeIds)
            {
                var history = new History
                {
                    EventType = HistoryEventType.DownloadFailed,
                    Date = DateTime.UtcNow,
                    Quality = message.Quality,
                    SourceTitle = message.SourceTitle,
                    SeriesId = message.SeriesId,
                    EpisodeId = episodeId,
                    DownloadId = message.DownloadId
                };

                history.Data.Add("DownloadClient", message.DownloadClient);
                history.Data.Add("Message", message.Message);

                _historyRepository.Insert(history);
            }

            if (message.MovieId > 0)
            {
                var history = new History
                {
                    EventType = HistoryEventType.DownloadFailed,
                    Date = DateTime.UtcNow,
                    Quality = message.Quality,
                    SourceTitle = message.SourceTitle,
                    MovieId = message.MovieId,
                    DownloadId = message.DownloadId
                };

                history.Data.Add("DownloadClient", message.DownloadClient);
                history.Data.Add("Message", message.Message);

                _historyRepository.Insert(history);
            }
        }

        public void Handle(EpisodeFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.NoLinkedEpisodes)
            {
                _logger.Debug("Removing episode file from DB as part of cleanup routine.");
                return;
            }

            if (message.EpisodeFile.SeriesId > 0)
            {
                foreach (var episode in message.EpisodeFile.Episodes.Value)
                {
                    var history = new History
                    {
                        EventType = HistoryEventType.EpisodeFileDeleted,
                        Date = DateTime.UtcNow,
                        Quality = message.EpisodeFile.Quality,
                        SourceTitle = message.EpisodeFile.Path,
                        SeriesId = message.EpisodeFile.SeriesId,
                        EpisodeId = episode.Id
                    };

                    history.Data.Add("Reason", message.Reason.ToString());

                    _historyRepository.Insert(history);
                }
            }
        }

        public void Handle(EpisodeGrabbedEvent message)
        {
            foreach (var episode in message.Episode.Episodes)
            {
                var history = new History
                {
                    EventType = HistoryEventType.Grabbed,
                    Date = DateTime.UtcNow,
                    Quality = message.Episode.ParsedInfo.Quality,
                    SourceTitle = message.Episode.Release.Title,
                    SeriesId = episode.SeriesId,
                    EpisodeId = episode.Id,
                    DownloadId = message.DownloadId
                };

                history.Data.Add("Indexer", message.Episode.Release.Indexer);
                history.Data.Add("NzbInfoUrl", message.Episode.Release.InfoUrl);
                history.Data.Add("ReleaseGroup", message.Episode.ParsedInfo.ReleaseGroup);
                history.Data.Add("Age", message.Episode.Release.Age.ToString());
                history.Data.Add("AgeHours", message.Episode.Release.AgeHours.ToString());
                history.Data.Add("AgeMinutes", message.Episode.Release.AgeMinutes.ToString());
                history.Data.Add("PublishedDate", message.Episode.Release.PublishDate.ToString("s") + "Z");
                history.Data.Add("DownloadClient", message.DownloadClient);
                history.Data.Add("Size", message.Episode.Release.Size.ToString());
                history.Data.Add("DownloadUrl", message.Episode.Release.DownloadUrl);
                history.Data.Add("Guid", message.Episode.Release.Guid);
                history.Data.Add("TvRageId", message.Episode.Release.TvRageId.ToString());
                history.Data.Add("Protocol", ((int)message.Episode.Release.DownloadProtocol).ToString());

                if (!message.Episode.ParsedInfo.ReleaseHash.IsNullOrWhiteSpace())
                {
                    history.Data.Add("ReleaseHash", message.Episode.ParsedInfo.ReleaseHash);
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
                var history = new History
                    {
                        EventType = HistoryEventType.DownloadFolderImported,
                        Date = DateTime.UtcNow,
                        Quality = message.EpisodeInfo.Quality,
                        SourceTitle = message.ImportedEpisode.SceneName ?? Path.GetFileNameWithoutExtension(message.EpisodeInfo.Path),
                        SeriesId = message.ImportedEpisode.SeriesId,
                        EpisodeId = episode.Id,
                        DownloadId = downloadId
                    };

                //Won't have a value since we publish this event before saving to DB.
                //history.Data.Add("FileId", message.ImportedEpisode.Id.ToString());
                history.Data.Add("DroppedPath", message.EpisodeInfo.Path);
                history.Data.Add("ImportedPath", Path.Combine(message.EpisodeInfo.Series.Path, message.ImportedEpisode.RelativePath));
                history.Data.Add("DownloadClient", message.DownloadClient);

                _historyRepository.Insert(history);
            }
        }

        public void Handle(MovieFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.NoLinkedEpisodes)
            {
                _logger.Debug("Removing movie file from DB as part of cleanup routine.");
                return;
            }

            if (message.MovieFile.MovieId > 0)
            {
                var history = new History
                {
                    EventType = HistoryEventType.EpisodeFileDeleted,
                    Date = DateTime.UtcNow,
                    Quality = message.MovieFile.Quality,
                    SourceTitle = message.MovieFile.Path,
                    MovieId = message.MovieFile.MovieId
                };

                history.Data.Add("Reason", message.Reason.ToString());

                _historyRepository.Insert(history);
            }
        }

        public void Handle(MovieGrabbedEvent message)
        {
            var movie = message.Movie;

            var history = new History
            {
                EventType = HistoryEventType.Grabbed,
                Date = DateTime.UtcNow,
                Quality = message.Movie.ParsedInfo.Quality,
                SourceTitle = message.Movie.Release.Title,
                MovieId = movie.Movie.Id,
                DownloadId = message.DownloadId
            };

            history.Data.Add("Indexer", message.Movie.Release.Indexer);
            history.Data.Add("NzbInfoUrl", message.Movie.Release.InfoUrl);
            history.Data.Add("ReleaseGroup", message.Movie.ParsedInfo.ReleaseGroup);
            history.Data.Add("Age", message.Movie.Release.Age.ToString());
            history.Data.Add("AgeHours", message.Movie.Release.AgeHours.ToString());
            history.Data.Add("AgeMinutes", message.Movie.Release.AgeMinutes.ToString());
            history.Data.Add("PublishedDate", message.Movie.Release.PublishDate.ToString("s") + "Z");
            history.Data.Add("DownloadClient", message.DownloadClient);
            history.Data.Add("Size", message.Movie.Release.Size.ToString());
            history.Data.Add("DownloadUrl", message.Movie.Release.DownloadUrl);
            history.Data.Add("Guid", message.Movie.Release.Guid);
            history.Data.Add("TvRageId", message.Movie.Release.TvRageId.ToString());
            history.Data.Add("Protocol", ((int)message.Movie.Release.DownloadProtocol).ToString());

            if (!message.Movie.ParsedInfo.ReleaseHash.IsNullOrWhiteSpace())
            {
                history.Data.Add("ReleaseHash", message.Movie.ParsedInfo.ReleaseHash);
            }

            var torrentRelease = message.Movie.Release as TorrentInfo;

            if (torrentRelease != null)
            {
                history.Data.Add("TorrentInfoHash", torrentRelease.InfoHash);
            }

            _historyRepository.Insert(history);
        }

        public void Handle(MovieImportedEvent message)
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

            var movie = message.MovieInfo;
            var history = new History
            {
                EventType = HistoryEventType.DownloadFolderImported,
                Date = DateTime.UtcNow,
                Quality = message.MovieInfo.Quality,
                SourceTitle = message.ImportedMovie.SceneName ?? Path.GetFileNameWithoutExtension(message.MovieInfo.Path),
                MovieId = message.ImportedMovie.Id,
                DownloadId = downloadId
            };

            //Won't have a value since we publish this event before saving to DB.
            //history.Data.Add("FileId", message.ImportedEpisode.Id.ToString());
            history.Data.Add("DroppedPath", message.MovieInfo.Path);
            history.Data.Add("ImportedPath", Path.Combine(message.MovieInfo.Movie.Path, message.ImportedMovie.RelativePath));
            history.Data.Add("DownloadClient", message.DownloadClient);

            _historyRepository.Insert(history);
        }

        public PagingSpec<History> Paged(PagingSpec<History> pagingSpec)
        {
            return _historyRepository.GetPaged(pagingSpec);
        }

        public History MostRecentForEpisode(int episodeId)
        {
            return _historyRepository.MostRecentForEpisode(episodeId);
        }

        public History MostRecentForMovie(int movieId)
        {
            return _historyRepository.MostRecentForMovie(movieId);
        }

        public History MostRecentForDownloadId(string downloadId)
        {
            return _historyRepository.MostRecentForDownloadId(downloadId);
        }

        public History Get(int historyId)
        {
            return _historyRepository.Get(historyId);
        }

        public List<History> Find(string downloadId, HistoryEventType eventType)
        {
            return _historyRepository.FindByDownloadId(downloadId).Where(c => c.EventType == eventType).ToList();
        }

        public List<History> FindByDownloadId(string downloadId)
        {
            return _historyRepository.FindByDownloadId(downloadId);
        }

        public QualityModel GetBestEpisodeQualityInHistory(Profile profile, int episodeId)
        {
            var comparer = new QualityModelComparer(profile);
            return _historyRepository.GetBestQualityInHistory(episodeId)
                .OrderByDescending(q => q, comparer)
                .FirstOrDefault();
        }

        public QualityModel GetBestMovieQualityInHistory(Profile profile, int movieId)
        {
            var comparer = new QualityModelComparer(profile);
            return _historyRepository.GetBestMovieQualityInHistory(movieId)
                .OrderByDescending(q => q, comparer)
                .FirstOrDefault();
        }

        private string FindDownloadId(EpisodeImportedEvent trackedDownload)
        {
            _logger.Debug("Trying to find downloadId for {0} from history", trackedDownload.ImportedEpisode.Path);

            var episodeIds = trackedDownload.EpisodeInfo.Episodes.Select(c => c.Id).ToList();

            var allHistory = _historyRepository.FindSeriesDownloadHistory(trackedDownload.EpisodeInfo.Series.Id, trackedDownload.ImportedEpisode.Quality);


            //Find download related items for these episdoes
            var episodesHistory = allHistory.Where(h => episodeIds.Contains(h.EpisodeId)).ToList();

            var processedDownloadId = episodesHistory
                .Where(c => c.EventType != HistoryEventType.Grabbed && c.DownloadId != null)
                .Select(c => c.DownloadId);

            var stillDownloading = episodesHistory.Where(c => c.EventType == HistoryEventType.Grabbed && !processedDownloadId.Contains(c.DownloadId)).ToList();

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

        private string FindDownloadId(MovieImportedEvent trackedDownload)
        {
            _logger.Debug("Trying to find downloadId for {0} from history", trackedDownload.ImportedMovie.Path);

            var movieId = trackedDownload.MovieInfo.Movie.Id;

            var allHistory = _historyRepository.FindMovieDownloadHistory(trackedDownload.MovieInfo.Movie.Id, trackedDownload.ImportedMovie.Quality);

            var processedDownloadId = allHistory
                .Where(c => c.EventType != HistoryEventType.Grabbed && c.DownloadId != null)
                .Select(c => c.DownloadId);

            var stillDownloading = allHistory.Where(c => c.EventType == HistoryEventType.Grabbed && !processedDownloadId.Contains(c.DownloadId)).ToList();

            string downloadId = null;

            if (stillDownloading.Any())
            {
                var matchingHistory = stillDownloading.Where(c => c.MovieId == trackedDownload.MovieInfo.Movie.Id).ToList();

                if (matchingHistory.Count != 1)
                {
                    return null;
                }

                var newDownloadId = matchingHistory.Single().DownloadId;

                downloadId = newDownloadId;
            }

            return downloadId;
        }
    }
}