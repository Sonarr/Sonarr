using System.Linq;
using NLog;
using Ninject;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Providers.DecisionEngine
{
    public class MonitoredEpisodeSpecification
    {
        private readonly SeriesProvider _seriesProvider;
        private readonly EpisodeProvider _episodeProvider;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public MonitoredEpisodeSpecification(SeriesProvider seriesProvider, EpisodeProvider episodeProvider)
        {
            _seriesProvider = seriesProvider;
            _episodeProvider = episodeProvider;
        }

        public MonitoredEpisodeSpecification()
        {
            
        }

        public virtual bool IsSatisfiedBy(EpisodeParseResult subject)
        {
            var series = _seriesProvider.FindSeries(subject.CleanTitle);

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
