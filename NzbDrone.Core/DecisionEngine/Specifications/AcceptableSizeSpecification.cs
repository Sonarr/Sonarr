using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class AcceptableSizeSpecification : IFetchableSpecification
    {
        private readonly IQualitySizeService _qualityTypeProvider;
        private readonly IEpisodeService _episodeService;
        private readonly Logger _logger;

        public AcceptableSizeSpecification(IQualitySizeService qualityTypeProvider, IEpisodeService episodeService, Logger logger)
        {
            _qualityTypeProvider = qualityTypeProvider;
            _episodeService = episodeService;
            _logger = logger;
        }

        public string RejectionReason
        {
            get { return "File size too big or small"; }
        }

        public virtual bool IsSatisfiedBy(EpisodeParseResult subject)
        {
            _logger.Trace("Beginning size check for: {0}", subject);

            if (subject.Quality.Quality == Quality.RAWHD)
            {
                _logger.Trace("Raw-HD release found, skipping size check.");
                return true;
            }

            var qualityType = _qualityTypeProvider.Get((int)subject.Quality.Quality);

            if (qualityType.MaxSize == 0)
            {
                _logger.Trace("Max size is 0 (unlimited) - skipping check.");
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
                _episodeService.IsFirstOrLastEpisodeOfSeason(series.Id,
                subject.SeasonNumber, subject.EpisodeNumbers[0]))
            {
                maxSize = maxSize * 2;
            }

            //If the parsed size is greater than maxSize we don't want it
            if (subject.Size > maxSize)
            {
                _logger.Trace("Item: {0}, Size: {1} is greater than maximum allowed size ({2}), rejecting.", subject, subject.Size, maxSize);
                return false;
            }

            _logger.Trace("Item: {0}, meets size constraints.", subject);
            return true;
        }

    }
}
