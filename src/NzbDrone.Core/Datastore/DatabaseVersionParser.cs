using System;
using System.Text.RegularExpressions;

namespace NzbDrone.Core.Datastore;

public static class DatabaseVersionParser
{
    private static readonly Regex VersionRegex = new (@"^[^ ]+", RegexOptions.Compiled);

    public static Version ParseServerVersion(string serverVersion)
    {
        var match = VersionRegex.Match(serverVersion);

        return match.Success ? new Version(match.Value) : null;
    }
}
