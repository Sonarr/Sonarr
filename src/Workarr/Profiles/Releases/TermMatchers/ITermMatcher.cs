namespace Workarr.Profiles.Releases.TermMatchers
{
    public interface ITermMatcher
    {
        bool IsMatch(string value);
        string MatchingTerm(string value);
    }
}
