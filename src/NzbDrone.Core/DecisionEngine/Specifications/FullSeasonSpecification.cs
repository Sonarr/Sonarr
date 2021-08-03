using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class FullSeasonSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public FullSeasonSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
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
