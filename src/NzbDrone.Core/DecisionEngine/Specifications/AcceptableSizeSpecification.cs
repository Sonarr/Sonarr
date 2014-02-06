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
        private readonly IQualityDefinitionService _qualityDefinitionService;
        private readonly IEpisodeService _episodeService;
        private readonly Logger _logger;

        public AcceptableSizeSpecification(IQualityDefinitionService qualityDefinitionService, IEpisodeService episodeService, Logger logger)
        {
            _qualityDefinitionService = qualityDefinitionService;
            _episodeService = episodeService;
            _logger = logger;
        }

        public string RejectionReason
        {
            get { return "File size too big or small"; }
        }

        public virtual bool IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
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

            var qualityDefinition = _qualityDefinitionService.Get(quality);

            if (qualityDefinition.MaxSize == 0)
            {
                _logger.Trace("Max size is 0 (unlimited) - skipping check.");
                return true;
            }

            var maxSize = qualityDefinition.MaxSize.Megabytes();

            //Multiply maxSize by Series.Runtime
            maxSize = maxSize * subject.Series.Runtime * subject.Episodes.Count;

            //Check if there was only one episode parsed and it is the first
            if (subject.Episodes.Count == 1 && subject.Episodes.First().EpisodeNumber == 1)
            {
                maxSize = maxSize * 2;
            }

            //If the parsed size is greater than maxSize we don't want it
            if (subject.Release.Size > maxSize)
            {
                _logger.Trace("Item: {0}, Size: {1} is greater than maximum allowed size ({2}), rejecting.", subject, subject.Release.Size, maxSize);
                return false;
            }

            _logger.Trace("Item: {0}, meets size constraints.", subject);
            return true;
        }

    }
}
