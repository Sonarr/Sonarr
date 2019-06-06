using System;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Profiles.Releases
{
    public class OriginReleaseFilter : IReleaseFilter
    {
        public string Key => "Origin";

        public bool Matches(string filterValue, RemoteEpisode subject)
        {
            return filterValue.Equals(subject.Release.Origin, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
