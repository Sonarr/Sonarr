namespace NzbDrone.Core.Profiles.Releases.TermMatchers
{
    public interface ITermMatcher
    {
        bool IsMatch(string value);
        string MatchingTerm(string value);
    }
}
