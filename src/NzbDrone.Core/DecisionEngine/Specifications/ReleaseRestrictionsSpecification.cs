using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Releases;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class ReleaseRestrictionsSpecification : IDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly IReleaseProfileService _releaseProfileService;
        private readonly ITermMatcherService _termMatcherService;

        public ReleaseRestrictionsSpecification(ITermMatcherService termMatcherService, IReleaseProfileService releaseProfileService, Logger logger)
        {
            _logger = logger;
            _releaseProfileService = releaseProfileService;
            _termMatcherService = termMatcherService;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual Decision IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            _logger.Debug("Checking if release meets restrictions: {0}", subject);

            var title = subject.Release.Title;
            var releaseProfiles = _releaseProfileService.EnabledForTags(subject.Series.Tags, subject.Release.IndexerId);

            var required = releaseProfiles.Where(r => r.Required.Any());
            var ignored = releaseProfiles.Where(r => r.Ignored.Any());

            foreach (var r in required)
            {
                var requiredTerms = r.Required;

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
                var ignoredTerms = r.Ignored;

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

        private List<string> ContainsAny(List<string> terms, string title)
        {
            return terms.Where(t => _termMatcherService.IsMatch(t, title)).ToList();
        }
    }
}
