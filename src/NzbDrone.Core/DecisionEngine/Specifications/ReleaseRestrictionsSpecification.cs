using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Restrictions;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class ReleaseRestrictionsSpecification : IDecisionEngineSpecification
    {
        private readonly IRestrictionService _restrictionService;
        private readonly Logger _logger;

        public ReleaseRestrictionsSpecification(IRestrictionService restrictionService, Logger logger)
        {
            _restrictionService = restrictionService;
            _logger = logger;
        }

        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            _logger.Debug("Checking if release meets restrictions: {0}", subject);

            var title = subject.Release.Title;
            var restrictions = _restrictionService.AllForTags(subject.Series.Tags);

            var required = restrictions.Where(r => r.Required.IsNotNullOrWhiteSpace());
            var ignored = restrictions.Where(r => r.Ignored.IsNotNullOrWhiteSpace());

            foreach (var r in required)
            {
                var requiredTerms = r.Required.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList();

                var foundTerms = ContainsAny(requiredTerms, title);
                if (foundTerms.Empty())
                {
                    var terms = string.Join(", ", requiredTerms);
                    _logger.Debug("[{0}] does not contain one of the required terms: {1}", title, terms);
                    return Decision.Reject("Does not contain one of the required terms: {0}", terms);
                }
            }

            foreach (var r in ignored)
            {
                var ignoredTerms = r.Ignored.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                var foundTerms = ContainsAny(ignoredTerms, title);
                if (foundTerms.Any())
                {
                    var terms = string.Join(", ", foundTerms);
                    _logger.Debug("[{0}] contains these ignored terms: {1}", title, terms);
                    return Decision.Reject("Contains these ignored terms: {0}", terms);
                }
            }

            _logger.Debug("[{0}] No restrictions apply, allowing", subject);
            return Decision.Accept();
        }

        private static List<string> ContainsAny(List<string> terms, string title)
        {
            return terms.Where(t => title.ToLowerInvariant().Contains(t.ToLowerInvariant())).ToList();
        }
    }
}
