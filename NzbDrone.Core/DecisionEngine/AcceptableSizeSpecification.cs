using System.Linq;
using NLog;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Core.DecisionEngine
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

            if(subject.Quality.Quality == QualityTypes.RAWHD)
            {
                logger.Trace("Raw-HD release found, skipping size check.");
                return true;
            }

            var qualityType = _qualityTypeProvider.Get((int)subject.Quality.Quality);

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
                logger.Trace("Item: {0}, Size: {1} is greater than maximum allowed size ({2}), rejecting.", subject, subject.Size, maxSize);
                return false;
            }
                
            logger.Trace("Item: {0}, meets size contraints.", subject);
            return true;
        }
       
    }
}
