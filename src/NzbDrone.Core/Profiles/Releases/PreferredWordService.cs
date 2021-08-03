using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Profiles.Releases
{
    public interface IPreferredWordService
    {
        int Calculate(Series series, string title, int indexerId);
        List<KeyValuePair<string, int>> GetMatchingPreferredWordsAndScores(Series series, string title, int indexerId);
        PreferredWordMatchResults GetMatchingPreferredWords(Series series, string title);
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

            var matchingPairs = GetMatchingPreferredWordsAndScores(series, title, indexerId);
            var score = matchingPairs.Sum(p => p.Value);

            _logger.Trace("Calculated preferred word score for '{0}': {1} ({2} match(es))", title, score, matchingPairs.Count);

            return score;
        }

        public List<KeyValuePair<string, int>> GetMatchingPreferredWordsAndScores(Series series, string title, int indexerId)
        {
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

            return matchingPairs;
        }

        public PreferredWordMatchResults GetMatchingPreferredWords(Series series, string title)
        {
            var releaseProfiles = _releaseProfileService.EnabledForTags(series.Tags, 0);
            var profileWords = new Dictionary<string, List<KeyValuePair<string, int>>>();
            var allWords = new List<KeyValuePair<string, int>>();

            _logger.Trace("Determining preferred word matches for '{0}'", title);

            foreach (var releaseProfile in releaseProfiles)
            {
                var matchingPairs = new List<KeyValuePair<string, int>>();

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

                if (matchingPairs.Count > 0)
                {
                    if (releaseProfile.Name.IsNotNullOrWhiteSpace())
                    {
                        var profileName = releaseProfile.Name.Trim();

                        if (!profileWords.ContainsKey(profileName))
                        {
                            profileWords.Add(profileName, new List<KeyValuePair<string, int>>());
                        }

                        profileWords[profileName].AddRange(matchingPairs);
                    }

                    allWords.AddRange(matchingPairs); // Add the "everything grouping"
                }
            }

            var results = new PreferredWordMatchResults()
            {
                All = allWords.OrderByDescending(m => m.Value).Select(m => m.Key).ToList(),
                ByReleaseProfile = profileWords.ToDictionary(item => item.Key, item => item.Value.OrderByDescending(m => m.Value).Select(m => m.Key).ToList())
            };

            _logger.Trace("Determined preferred word matches for '{0}'. Count {1}", title, allWords.Count);

            return results;
        }
  }
}
