using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NzbDrone.Common.Cache;

namespace NzbDrone.Core.Restrictions
{
    public interface ITermMatcher
    {
        bool IsMatch(string term, string value);
    }

    public class TermMatcher : ITermMatcher
    {
        private ICached<Predicate<string>> _matcherCache;

        public TermMatcher(ICacheManager cacheManager)
        {
            _matcherCache = cacheManager.GetCache<Predicate<string>>(GetType());
        }

        public bool IsMatch(string term, string value)
        {
            return GetMatcher(term)(value);
        }

        public Predicate<string> GetMatcher(string term)
        {
            return _matcherCache.Get(term, () => CreateMatcherInternal(term), TimeSpan.FromHours(24));
        }

        private Predicate<string> CreateMatcherInternal(string term)
        {
            Regex regex;
            if (PerlRegexFactory.TryCreateRegex(term, out regex))
            {
                return regex.IsMatch;
            }
            else
            {
                return new CaseInsensitiveTermMatcher(term).IsMatch;

            }
        }

        private sealed class CaseInsensitiveTermMatcher
        {
            private readonly string _term;

            public CaseInsensitiveTermMatcher(string term)
            {
                _term = term.ToLowerInvariant();
            }

            public bool IsMatch(string value)
            {
                return value.ToLowerInvariant().Contains(_term);
            }
        }
    }
}
