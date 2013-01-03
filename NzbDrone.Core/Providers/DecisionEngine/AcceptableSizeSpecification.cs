using System.Linq;
using NLog;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Providers.DecisionEngine
{
    public class AcceptableSizeSpecification
    {
        private readonly QualityTypeProvider _qualityTypeProvider;
        private readonly EpisodeProvider _episodeProvider;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public AcceptableSizeSpecification(QualityTypeProvider qualityTypeProvider, EpisodeProvider episodeProvider)
        {
            _qualityTypeProvider = qualityTypeProvider;
            _episodeProvider = episodeProvider;
        }

        public AcceptableSizeSpecification()
        {
            
        }

        public virtual bool IsSatisfiedBy(EpisodeParseResult subject)
        {
            logger.Trace("Beginning size check for: {0}", subject);
            var qualityType = _qualityTypeProvider.Get((int)subject.Quality.Quality);

            //Need to determine if this is a 30 or 60 minute episode
            //Is it a multi-episode release?
            //Is it the first or last series of a season?

            //0 will be treated as unlimited
            if (qualityType.MaxSize == 0)
            {
                logger.Trace("Max size is 0 (unlimited) - skipping check.");
                return true;
            }

            var maxSize = qualityType.MaxSize.Megabytes();
            var series = subject.Series;

            //Multiply maxSize by Series.Runtime
            maxSize = maxSize * series.Runtime;

            //Multiply maxSize by the number of episodes parsed (if EpisodeNumbers is null it will be treated as a single episode)
            //TODO: is this check really necessary? shouldn't we blowup?
            if (subject.EpisodeNumbers != null)
                maxSize = maxSize * subject.EpisodeNumbers.Count;

            //Check if there was only one episode parsed
            //and it is the first or last episode of the season
            if (subject.EpisodeNumbers != null && subject.EpisodeNumbers.Count == 1 &&
                _episodeProvider.IsFirstOrLastEpisodeOfSeason(series.SeriesId,
                subject.SeasonNumber, subject.EpisodeNumbers[0]))
            {
                maxSize = maxSize * 2;
            }

            //If the parsed size is greater than maxSize we don't want it
            if (subject.Size > maxSize)
            {
                logger.Trace("Item: {0}, Size: {1} is greater than maximum allowed size ({1}), rejecting.", subject, subject.Size, maxSize);
                return false;
            }
                
            logger.Trace("Item: {0}, meets size contraints.", subject);
            return true;
        }
       
    }
}
