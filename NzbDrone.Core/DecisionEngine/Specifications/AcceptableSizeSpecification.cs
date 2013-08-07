using System.Linq;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class AcceptableSizeSpecification : IDecisionEngineSpecification
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

        public virtual bool IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteriaBase)
        {

            _logger.Trace("Beginning size check for: {0}", subject);

            var quality = subject.ParsedEpisodeInfo.Quality.Quality;

            if (quality == Quality.RAWHD)
            {
                _logger.Trace("Raw-HD release found, skipping size check.");
                return true;
            }

            if (quality == Quality.Unknown)
            {
                _logger.Trace("Unknown quality. skipping size check.");
                return false;
            }

            var qualityType = _qualityTypeProvider.Get(quality.Id);

            if (qualityType.MaxSize == 0)
            {
                _logger.Trace("Max size is 0 (unlimited) - skipping check.");
                return true;
            }

            var maxSize = qualityType.MaxSize.Megabytes();

            //Multiply maxSize by Series.Runtime
            maxSize = maxSize * subject.Series.Runtime * subject.Episodes.Count;

            //Check if there was only one episode parsed
            //and it is the first or last episode of the season
            if (subject.Episodes.Count == 1 && _episodeService.IsFirstOrLastEpisodeOfSeason(subject.Episodes.Single().Id))
            {
                maxSize = maxSize * 2;
            }

            //If the parsed size is greater than maxSize we don't want it
            if (subject.Report.Size > maxSize)
            {
                _logger.Trace("Item: {0}, Size: {1} is greater than maximum allowed size ({2}), rejecting.", subject, subject.Report.Size, maxSize);
                return false;
            }

            _logger.Trace("Item: {0}, meets size constraints.", subject);
            return true;
        }

    }
}
