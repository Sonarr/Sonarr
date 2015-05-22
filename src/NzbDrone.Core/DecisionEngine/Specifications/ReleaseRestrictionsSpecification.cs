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

        public RejectionType Type { get { return RejectionType.Permanent; } }

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            _logger.Debug("Checking if release meets restrictions: {0}", subject);

            var title = subject.Release.Title;
            var restrictions = _restrictionService.AllForTags(subject.Series.Tags);

            var required = restrictions.Where(r => r.Required.IsNotNullOrWhiteSpace());
            var ignored = restrictions.Where(r => r.Ignored.IsNotNullOrWhiteSpace());

            foreach (var r in required)
            {
                var split = r.Required.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (!ContainsAny(split, title))
                {
                    _logger.Debug("[{0}] does not contain one of the required terms: {1}", title, r.Required);
                    return Decision.Reject("Does not contain one of the required terms: {0}", r.Required.Replace(",", ", "));
                }
            }

            foreach (var r in ignored)
            {
                var split = r.Ignored.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (ContainsAny(split, title))
                {
                    _logger.Debug("[{0}] contains one or more ignored terms: {1}", title, r.Ignored);
                    return Decision.Reject("Contains one or more ignored terms: {0}", r.Ignored.Replace(",", ", "));
                }
            }

            _logger.Debug("[{0}] No restrictions apply, allowing", subject);
            return Decision.Accept();
        }

        private static Boolean ContainsAny(List<String> terms, String title)
        {
            return terms.Any(t => title.ToLowerInvariant().Contains(t.ToLowerInvariant()));
        }
    }
}
