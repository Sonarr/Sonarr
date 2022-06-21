﻿using System;
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
            if (searchCriteria == null || searchCriteria.Episodes.Count == 1)
            {
                return Decision.Accept();
            }

            if (subject.Release.SeasonSearchMaximumSingleEpisodeAge > 0)
            {
                if (subject.Series.SeriesType == SeriesTypes.Standard && !subject.ParsedEpisodeInfo.FullSeason && subject.Episodes.Count >= 1)
                {
                    // test against episodes of the same season in the current search, and make sure they have an air date
                    var subset = searchCriteria.Episodes.Where(e => e.AirDateUtc.HasValue && e.SeasonNumber == subject.Episodes.First().SeasonNumber).ToList();

                    if (subset.Count() > 0 && subset.Max(e => e.AirDateUtc).Value.Before(DateTime.UtcNow - TimeSpan.FromDays(subject.Release.SeasonSearchMaximumSingleEpisodeAge)))
                    {
                        _logger.Debug("Release {0}: last episode in this season aired more than {1} days ago, season pack required.", subject.Release.Title, subject.Release.SeasonSearchMaximumSingleEpisodeAge);
                        return Decision.Reject("Last episode in this season aired more than {0} days ago, season pack required.", subject.Release.SeasonSearchMaximumSingleEpisodeAge);
                    }
                }
            }

            return Decision.Accept();
        }
    }
}
