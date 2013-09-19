using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.History
{
    public interface IHistoryService
    {
        List<History> All();
        void Purge();
        void Trim();
        QualityModel GetBestQualityInHistory(int episodeId);
        PagingSpec<History> Paged(PagingSpec<History> pagingSpec);
        void CleanupOrphaned();
    }

    public class HistoryService : IHistoryService, IHandle<EpisodeGrabbedEvent>, IHandle<EpisodeImportedEvent>
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

        public void CleanupOrphaned()
        {
            _historyRepository.CleanupOrphanedBySeries();
            _historyRepository.CleanupOrphanedByEpisode();
        }

        public void Purge()
        {
            _historyRepository.Purge();
        }

        public virtual void Trim()
        {
            _historyRepository.Trim();
        }

        public virtual QualityModel GetBestQualityInHistory(int episodeId)
        {
            return _historyRepository.GetEpisodeHistory(episodeId).OrderByDescending(q => q).FirstOrDefault();
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
                history.Data.Add("ReleaseGroup", message.Episode.Release.ReleaseGroup);
                history.Data.Add("Age", message.Episode.Release.Age.ToString());

                _historyRepository.Insert(history);
            }
        }

        public void Handle(EpisodeImportedEvent message)
        {
            foreach (var episode in message.DroppedEpisode.Episodes)
            {
                var history = new History
                    {
                        EventType = HistoryEventType.DownloadFolderImported,
                        Date = DateTime.UtcNow,
                        Quality = message.DroppedEpisode.Quality,
                        SourceTitle = message.ImportedEpisode.SceneName,
                        SeriesId = message.ImportedEpisode.SeriesId,
                        EpisodeId = episode.Id
                    };

                //Won't have a value since we publish this event before saving to DB.
                //history.Data.Add("FileId", message.ImportedEpisode.Id.ToString());
                history.Data.Add("DroppedPath", message.DroppedEpisode.Path);
                history.Data.Add("ImportedPath", message.ImportedEpisode.Path);

                _historyRepository.Insert(history);
            }
        }
    }
}