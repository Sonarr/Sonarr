using NzbDrone.Core.Parser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public abstract class BaseSeriesSpecification : ISeriesDecisionEngineSpecification
    {
        public abstract Decision IsSatisfiedBy(RemoteEpisode subject, SeriesSearchCriteriaBase searchCriteria);

        public abstract RejectionType Type { get; }

        public Decision IsSatisfiedBy(RemoteItem subject, SearchCriteriaBase searchCriteria)
        {
            if (subject is RemoteEpisode)
                return IsSatisfiedBy(subject as RemoteEpisode, searchCriteria as SeriesSearchCriteriaBase);
            return Decision.Accept();
        }
    }
}
