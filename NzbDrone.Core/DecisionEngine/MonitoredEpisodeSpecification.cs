using System.Linq;
using NLog;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;

namespace NzbDrone.Core.DecisionEngine
{
    public class MonitoredEpisodeSpecification
    {
        private readonly SeriesProvider _seriesProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly ISeriesRepository _seriesRepository;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public MonitoredEpisodeSpecification(SeriesProvider seriesProvider, EpisodeProvider episodeProvider, ISeriesRepository seriesRepository)
        {
            _seriesProvider = seriesProvider;
            _episodeProvider = episodeProvider;
            _seriesRepository = seriesRepository;
        }

        public MonitoredEpisodeSpecification()
        {
            
        }

        public virtual bool IsSatisfiedBy(EpisodeParseResult subject)
        {
            var series = _seriesRepository.Get(subject.CleanTitle);

            if (series == null)
            {
                logger.Trace("{0} is not mapped to any series in DB. skipping", subject.CleanTitle);
                return false;
            }

            subject.Series = series;

            if (!series.Monitored)
            {
                logger.Debug("{0} is present in the DB but not tracked. skipping.", subject.CleanTitle);
                return false;
            }

            var episodes = _episodeProvider.GetEpisodesByParseResult(subject);
            subject.Episodes = episodes;

            //return monitored if any of the episodes are monitored
            if (episodes.Any(episode => !episode.Ignored))
            {
                return true;
            }

            logger.Debug("All episodes are ignored. skipping.");
            return false;
        }
    }
}
