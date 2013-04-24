using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.History
{
    public interface IHistoryService
    {
        List<History> All();
        void Purge();
        void Trim();
        QualityModel GetBestQualityInHistory(int episodeId);
    }

    public class HistoryService : IHistoryService, IHandle<EpisodeGrabbedEvent>
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
            return _historyRepository.GetBestQualityInHistory(episodeId);
        }

        public void Handle(EpisodeGrabbedEvent message)
        {
            foreach (var episode in message.Episode.Episodes)
            {
                var history = new History
                {
                    Date = DateTime.Now,
                    Indexer = message.Episode.Report.Indexer,
                    Quality = message.Episode.Quality,
                    NzbTitle = message.Episode.Report.Title,
                    EpisodeId = episode.Id,
                    NzbInfoUrl = message.Episode.Report.NzbInfoUrl,
                    ReleaseGroup = message.Episode.Report.ReleaseGroup,
                };

                _historyRepository.Insert(history);
            }
        }
    }
}