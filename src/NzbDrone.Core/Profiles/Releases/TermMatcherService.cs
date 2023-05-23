using System;
using NzbDrone.Common.Cache;
using NzbDrone.Core.Profiles.Releases.TermMatchers;

namespace NzbDrone.Core.Profiles.Releases
{
    public interface ITermMatcherService
    {
        bool IsMatch(string term, string value);
        string MatchingTerm(string term, string value);
    }

    public class TermMatcherService : ITermMatcherService
    {
        private ICached<ITermMatcher> _matcherCache;

        public TermMatcherService(ICacheManager cacheManager)
        {
            _matcherCache = cacheManager.GetCache<ITermMatcher>(GetType());
        }

        public bool IsMatch(string term, string value)
        {
            return GetMatcher(term).IsMatch(value);
        }

        public string MatchingTerm(string term, string value)
        {
            return GetMatcher(term).MatchingTerm(value);
        }

        public ITermMatcher GetMatcher(string term)
        {
            return _matcherCache.Get(term, () => CreateMatcherInternal(term), TimeSpan.FromHours(24));
        }

        private ITermMatcher CreateMatcherInternal(string term)
        {
            if (PerlRegexFactory.TryCreateRegex(term, out var regex))
            {
                return new RegexTermMatcher(regex);
            }
            else
            {
                return new CaseInsensitiveTermMatcher(term);
            }
        }
    }
}
