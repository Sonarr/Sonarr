using System;
using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Common.Extensions;
using System.Linq;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class FullSeasonSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly IEpisodeService _episodeService;

        public FullSeasonSpecification(Logger logger, IEpisodeService episodeService)
        {
            _logger = logger;
            _episodeService = episodeService;
        }

        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (subject.ParsedEpisodeInfo.FullSeason)
            {
                _logger.Debug("Checking if all episodes in full season release have aired. {0}", subject.Release.Title);

                if (subject.Episodes.Any(e => !e.AirDateUtc.HasValue || e.AirDateUtc.Value.After(DateTime.UtcNow)))
                {
                    _logger.Debug("Full season release {0} rejected. All episodes haven't aired yet.", subject.Release.Title);
                    return Decision.Reject("Full season release rejected. All episodes haven't aired yet.");
                }
            }

            return Decision.Accept();
        }
    }
}
