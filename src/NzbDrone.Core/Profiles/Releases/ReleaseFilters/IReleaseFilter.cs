using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Profiles.Releases
{
    public interface IReleaseFilter
    {
        string Key { get; }
        bool Matches(string filterValue, RemoteEpisode subject);
        //List<string> SuggestAutoComplete(string filterValue, int indexerId);
    }
}
