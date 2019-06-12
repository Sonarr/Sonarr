using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        private static readonly Regex keyValueRegex = new Regex(@"^([^:]+):([^:]+)$");

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

            var required = releaseProfiles.Where(r => r.Required.IsNotNullOrWhiteSpace());
            var ignored = releaseProfiles.Where(r => r.Ignored.IsNotNullOrWhiteSpace());

            foreach (var r in required)
            {
                var requiredTerms = r.Required.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                // separate key-value terms and normal terms
                var reqKeyValues = requiredTerms.Where(kv => keyValueRegex.IsMatch(kv)).ToList();
                var reqTitleTerms = requiredTerms.Where(t => !keyValueRegex.IsMatch(t)).ToList();

                // check title terms
                var foundTerms = ContainsAny(reqTitleTerms, title);

                // check key-value terms
                foundTerms.AddRange(ContainsAnyKeyValues(reqKeyValues, subject));


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

                // separate key-value terms and normal terms
                var ignKeyValues = ignoredTerms.Where(kv => keyValueRegex.IsMatch(kv)).ToList();
                var ignTitleTerms = ignoredTerms.Where(t => !keyValueRegex.IsMatch(t)).ToList();

                // check title terms
                var foundTerms = ContainsAny(ignTitleTerms, title);

                // check key-value terms
                foundTerms.AddRange(ContainsAnyKeyValues(ignKeyValues, subject));

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

        private List<string> ContainsAnyKeyValues(List<string> terms, RemoteEpisode subject)
        {
            var foundTerms = new List<string>(); 

            foreach (var kv in terms)
            {
                var match = keyValueRegex.Match(kv);
                var key = match.Groups[1].Value;
                var value = match.Groups[2].Value;

                try
                {
                    IReleaseFilter releaseFilter = Assembly.GetExecutingAssembly().
                        CreateInstance("NzbDrone.Core.Profiles.Releases." + key + "ReleaseFilter", true) as IReleaseFilter;

                    if (releaseFilter.Matches(value, subject))
                    {
                        foundTerms.Add(kv);
                    }
                }
                catch (NullReferenceException)
                {
                    _logger.Debug("Unsupported key {0}", key);
                }
            }

            return foundTerms;
        }
    }
}
