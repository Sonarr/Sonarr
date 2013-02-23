using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.History
{

    public class HistoryService
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

        public void Add(History item)
        {

        }

        public virtual QualityModel GetBestQualityInHistory(int seriesId, int seasonNumber, int episodeNumber)
        {
            return _historyRepository.GetBestQualityInHistory(seriesId, seasonNumber, episodeNumber);
        }

    }
}