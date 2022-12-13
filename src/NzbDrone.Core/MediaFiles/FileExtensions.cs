using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MediaFiles
{
    internal static class FileExtensions
    {
        private static List<string> _archiveExtensions = new List<string>
        {
            ".rar",
            ".r00",
            ".zip",
            ".tar",
            ".gz",
            ".tar.gz"
        };

        private static List<string> _executableExtensions = new List<string>
        {
            ".exe",
            ".bat",
            ".cmd",
            ".sh"
        };

        public static HashSet<string> ArchiveExtensions => new HashSet<string>(_archiveExtensions, StringComparer.OrdinalIgnoreCase);
        public static HashSet<string> ExecutableExtensions => new HashSet<string>(_executableExtensions, StringComparer.OrdinalIgnoreCase);
    }
}
