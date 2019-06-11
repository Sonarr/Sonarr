using NLog;
using NzbDrone.Core.Tv;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Profiles.Releases
{
    public interface IPreferredWordService
    {
        int Calculate(Series series, string title, int indexerId);
        List<string> GetMatchingPreferredWords(Series series, string title);
    }

    public class PreferredWordService : IPreferredWordService
    {
        private readonly IReleaseProfileService _releaseProfileService;
        private readonly ITermMatcherService _termMatcherService;
        private readonly Logger _logger;

        public PreferredWordService(IReleaseProfileService releaseProfileService, ITermMatcherService termMatcherService, Logger logger)
        {
            _releaseProfileService = releaseProfileService;
            _termMatcherService = termMatcherService;
            _logger = logger;
        }

        public int Calculate(Series series, string title, int indexerId)
        {
            _logger.Trace("Calculating preferred word score for '{0}'", title);

            var releaseProfiles = _releaseProfileService.EnabledForTags(series.Tags, indexerId);
            var matchingPairs = new List<KeyValuePair<string, int>>();

            foreach (var releaseProfile in releaseProfiles)
            {
                foreach (var preferredPair in releaseProfile.Preferred)
                {
                    var term = preferredPair.Key;

                    if (_termMatcherService.IsMatch(term, title))
                    {
                        matchingPairs.Add(preferredPair);
                    }
                }
            }

            var score = matchingPairs.Sum(p => p.Value);

            _logger.Trace("Calculated preferred word score for '{0}': {1}", title, score);

            return score;
        }

        public List<string> GetMatchingPreferredWords(Series series, string title)
        {
            var releaseProfiles = _releaseProfileService.EnabledForTags(series.Tags, 0);
            var matchingPairs = new List<KeyValuePair<string, int>>();

            _logger.Trace("Calculating preferred word score for '{0}'", title);

            foreach (var releaseProfile in releaseProfiles)
            {
                if (!releaseProfile.IncludePreferredWhenRenaming)
                {
                    continue;
                }

                foreach (var preferredPair in releaseProfile.Preferred)
                {
                    var term = preferredPair.Key;
                    var matchingTerm = _termMatcherService.MatchingTerm(term, title);

                    if (matchingTerm.IsNotNullOrWhiteSpace())
                    {
                        matchingPairs.Add(new KeyValuePair<string, int>(matchingTerm, preferredPair.Value));
                    }
                }
            }

            return matchingPairs.OrderByDescending(p => p.Value)
                                .Select(p => p.Key)
                                .ToList();
        }
    }
}
