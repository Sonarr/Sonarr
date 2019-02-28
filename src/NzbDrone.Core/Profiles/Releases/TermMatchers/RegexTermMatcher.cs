using System.Text.RegularExpressions;

namespace NzbDrone.Core.Profiles.Releases.TermMatchers
{
    public class RegexTermMatcher : ITermMatcher
    {
        private readonly Regex _regex;

        public RegexTermMatcher(Regex regex)
        {
            _regex = regex;
        }

        public bool IsMatch(string value)
        {
            return _regex.IsMatch(value);
        }

        public string MatchingTerm(string value)
        {
            return _regex.Match(value).Value;
        }
    }
}
