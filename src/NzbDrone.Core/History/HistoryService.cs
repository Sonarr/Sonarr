using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.History
{
    public interface IHistoryService
    {
        List<History> All();
        void Purge();
        void Trim();
        QualityModel GetBestQualityInHistory(QualityProfile qualityProfile, int episodeId);
        PagingSpec<History> Paged(PagingSpec<History> pagingSpec);
        List<History> BetweenDates(DateTime startDate, DateTime endDate, HistoryEventType eventType);
        List<History> Failed();
        List<History> Grabbed();
        List<History> Imported();
        History MostRecentForEpisode(int episodeId);
        History Get(int id);
        List<History> FindBySourceTitle(string sourceTitle);
        void UpdateHistoryData(Int32 historyId, Dictionary<String, String> data);
    }

    public class HistoryService : IHistoryService, IHandle<EpisodeGrabbedEvent>, IHandle<EpisodeImportedEvent>, IHandle<DownloadFailedEvent>
    {
        private readonly IHistoryRepository _historyRepository;
        private readonly Logger _logger;

        public HistoryService(IHistoryRepository historyRepository, Logger logger)
        {
            _historyRepository = historyRepository;
            _logger = logger;
        }

        public List<History> All()
        {
            return _historyRepository.All().ToList();
        }

        public PagingSpec<History> Paged(PagingSpec<History> pagingSpec)
        {
            return _historyRepository.GetPaged(pagingSpec);
        }

        public List<History> BetweenDates(DateTime startDate, DateTime endDate, HistoryEventType eventType)
        {
            return _historyRepository.BetweenDates(startDate, endDate, eventType);
        }

        public List<History> Failed()
        {
            return _historyRepository.Failed();
        }

        public List<History> Grabbed()
        {
            return _historyRepository.Grabbed();
        }

        public List<History> Imported()
        {
            return _historyRepository.Imported();
        }

        public History MostRecentForEpisode(int episodeId)
        {
            return _historyRepository.MostRecentForEpisode(episodeId);
        }

        public History Get(int id)
        {
            return _historyRepository.Get(id);
        }

        public List<History> FindBySourceTitle(string sourceTitle)
        {
            return _historyRepository.FindBySourceTitle(sourceTitle);
        }

        public void Purge()
        {
            _historyRepository.Purge();
        }

        public virtual void Trim()
        {
            _historyRepository.Trim();
        }

        public QualityModel GetBestQualityInHistory(QualityProfile qualityProfile, int episodeId)
        {
            var comparer = new QualityModelComparer(qualityProfile);
            return _historyRepository.GetBestQualityInHistory(episodeId)
                .OrderByDescending(q => q, comparer)
                .FirstOrDefault();
        }

        public void UpdateHistoryData(Int32 historyId, Dictionary<String, String> data)
        {
            var history = _historyRepository.Get(historyId);
            history.Data = data;
            _historyRepository.Update(history);
        }

        public void Handle(EpisodeGrabbedEvent message)
        {
            foreach (var episode in message.Episode.Episodes)
            {
                var history = new History
                {
                    EventType = HistoryEventType.Grabbed,
                    Date = DateTime.UtcNow,
                    Quality = message.Episode.ParsedEpisodeInfo.Quality,
                    SourceTitle = message.Episode.Release.Title,
                    SeriesId = episode.SeriesId,
                    EpisodeId = episode.Id,
                };

                history.Data.Add("Indexer", message.Episode.Release.Indexer);
                history.Data.Add("NzbInfoUrl", message.Episode.Release.InfoUrl);
                history.Data.Add("ReleaseGroup", message.Episode.ParsedEpisodeInfo.ReleaseGroup);
                history.Data.Add("Age", message.Episode.Release.Age.ToString());
                history.Data.Add("AgeHours", message.Episode.Release.AgeHours.ToString());
                history.Data.Add("PublishedDate", message.Episode.Release.PublishDate.ToString("s") + "Z");
                history.Data.Add("DownloadClient", message.DownloadClient);

                if (!String.IsNullOrWhiteSpace(message.DownloadClientId))
                {
                    history.Data.Add("DownloadClientId", message.DownloadClientId);
                }

                if (!message.Episode.ParsedEpisodeInfo.ReleaseHash.IsNullOrWhiteSpace())
                {
                    history.Data.Add("ReleaseHash", message.Episode.ParsedEpisodeInfo.ReleaseHash);
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

            foreach (var episode in message.EpisodeInfo.Episodes)
            {
                var history = new History
                    {
                        EventType = HistoryEventType.DownloadFolderImported,
                        Date = DateTime.UtcNow,
                        Quality = message.EpisodeInfo.Quality,
                        SourceTitle = message.ImportedEpisode.SceneName,
                        SeriesId = message.ImportedEpisode.SeriesId,
                        EpisodeId = episode.Id
                    };

                //Won't have a value since we publish this event before saving to DB.
                //history.Data.Add("FileId", message.ImportedEpisode.Id.ToString());
                history.Data.Add("DroppedPath", message.EpisodeInfo.Path);
                history.Data.Add("ImportedPath", message.ImportedEpisode.Path);
                history.Data.Add("DownloadClient", message.DownloadClient);
                history.Data.Add("DownloadClientId", message.DownloadClientId);

                _historyRepository.Insert(history);
            }
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
                };

                history.Data.Add("DownloadClient", message.DownloadClient);
                history.Data.Add("DownloadClientId", message.DownloadClientId);
                history.Data.Add("Message", message.Message);

                _historyRepository.Insert(history);
            }
        }
    }
}