using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Events;
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
                    EventType =  HistoryEventType.Grabbed,
                    Date = DateTime.UtcNow,
                    Quality = message.Episode.ParsedEpisodeInfo.Quality,
                    SourceTitle = message.Episode.Report.Title,
                    SeriesId = episode.SeriesId,
                    EpisodeId = episode.Id,
                };

                history.Data.Add("Indexer", message.Episode.Report.Indexer);
                history.Data.Add("NzbInfoUrl", message.Episode.Report.NzbInfoUrl);
                history.Data.Add("ReleaseGroup", message.Episode.Report.ReleaseGroup);
                history.Data.Add("Age", message.Episode.Report.Age.ToString());

                _historyRepository.Insert(history);
            }
        }

        public void Handle(EpisodeImportedEvent message)
        {
            foreach (var episode in message.EpisodeFile.Episodes.Value)
            {
                var history = new History
                    {
                        EventType = HistoryEventType.DownloadFolderImported,
                        Date = DateTime.UtcNow,
                        Quality = message.EpisodeFile.Quality,
                        SourceTitle = message.EpisodeFile.Path,
                        SeriesId = message.EpisodeFile.SeriesId,
                        EpisodeId = episode.Id,
                    };

                _historyRepository.Insert(history);
            }
        }
    }
}