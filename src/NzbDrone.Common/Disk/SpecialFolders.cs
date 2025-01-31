using System.Collections.Generic;

namespace NzbDrone.Common.Disk;

public static class SpecialFolders
{
    private static readonly HashSet<string> _specialFolders = new HashSet<string>
    {
        // Windows
        "boot",
        "bootmgr",
        "cache",
        "msocache",
        "recovery",
        "$recycle.bin",
        "recycler",
        "system volume information",
        "temporary internet files",
        "windows",

        // OS X
        ".fseventd",
        ".spotlight",
        ".trashes",
        ".vol",
        "cachedmessages",
        "caches",
        "trash",

        // QNAP
        ".@__thumb",

        // Synology
        "@eadir",
        "#recycle"
    };

    public static bool IsSpecialFolder(string folder)
    {
        if (folder == null)
        {
            return false;
        }

        return _specialFolders.Contains(folder.ToLowerInvariant());
    }
}
