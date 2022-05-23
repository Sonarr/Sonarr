using System;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Common.Extensions;
using System.Linq;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class SeasonPackOnlySpecification : IDecisionEngineSpecification
    {
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public SeasonPackOnlySpecification(IConfigService configService, Logger logger)
        {
            _configService = configService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            if (!subject.ParsedEpisodeInfo.FullSeason)
            {
                if (_configService.MaximumSingleEpisodeAge > 0)
                {
                    if (!subject.Episodes.Any(e => e.AirDateUtc.HasValue && e.AirDateUtc.Value.After(DateTime.UtcNow - TimeSpan.FromDays(_configService.MaximumSingleEpisodeAge))))
                    {
                        _logger.Debug("Single episode release {0} rejected. All episodes in season are older than {1} days.", subject.Release.Title, _configService.MaximumSingleEpisodeAge);
                        return Decision.Reject("Single episode release rejected. All episodes in season are older than {0} days.", _configService.MaximumSingleEpisodeAge);
                    }
                }
            }


            return Decision.Accept();
        }
    }
}
