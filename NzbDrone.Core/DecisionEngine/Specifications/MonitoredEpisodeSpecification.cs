using System.Linq;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class MonitoredEpisodeSpecification : IDecisionEngineSpecification
    {
        private readonly IEpisodeService _episodeService;
        private readonly ISeriesRepository _seriesRepository;
        private readonly Logger _logger;

        public MonitoredEpisodeSpecification(IEpisodeService episodeService, ISeriesRepository seriesRepository, Logger logger)
        {
            _episodeService = episodeService;
            _seriesRepository = seriesRepository;
            _logger = logger;
        }

        public string RejectionReason
        {
            get
            {
                return "Series is not monitored";
            }
        }

        public virtual bool IsSatisfiedBy(EpisodeParseResult subject)
        {
            var series = _seriesRepository.GetByTitle(subject.CleanTitle);

            if (series == null)
            {
                _logger.Trace("{0} is not mapped to any series in DB. skipping", subject.CleanTitle);
                return false;
            }

            subject.Series = series;

            if (!series.Monitored)
            {
                _logger.Debug("{0} is present in the DB but not tracked. skipping.", subject.CleanTitle);
                return false;
            }

            var episodes = _episodeService.GetEpisodesByParseResult(subject);
            subject.Episodes = episodes;

            //return monitored if any of the episodes are monitored
            if (episodes.Any(episode => !episode.Ignored))
            {
                return true;
            }

            _logger.Debug("All episodes are ignored. skipping.");
            return false;
        }
    }
}
