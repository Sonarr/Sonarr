using System;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Common.Extensions;
using System.Linq;
using NzbDrone.Core.Tv;

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
            if (subject.Release.MaximumSingleEpisodeAge > 0)
            {
                if ((subject.Series.SeriesType != SeriesTypes.Anime && !subject.ParsedEpisodeInfo.FullSeason) 
                    || (subject.Series.SeriesType == SeriesTypes.Anime && subject.Episodes.Count() == 1))
                {
                    if (!subject.Episodes.Any(e => e.AirDateUtc.HasValue && e.AirDateUtc.Value.After(DateTime.UtcNow - TimeSpan.FromDays(subject.Release.MaximumSingleEpisodeAge))))
                    {
                        _logger.Debug("Single episode release {0} rejected because it's older than {1} days.", subject.Release.Title, subject.Release.MaximumSingleEpisodeAge);
                        return Decision.Reject("Single episode release rejected because it's older than {0} days.", subject.Release.MaximumSingleEpisodeAge);
                    }
                }
            }


            return Decision.Accept();
        }
    }
}
