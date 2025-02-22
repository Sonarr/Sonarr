using System;
using System.Collections.Generic;

namespace NzbDrone.Core.MediaFiles
{
    internal static class FileExtensions
    {
        private static List<string> _archiveExtensions = new List<string>
        {
            ".7z",
            ".bz2",
            ".gz",
            ".r00",
            ".rar",
            ".tar.bz2",
            ".tar.gz",
            ".tar",
            ".tb2",
            ".tbz2",
            ".tgz",
            ".zip"
        };

        private static List<string> _dangerousExtensions = new List<string>
        {
            ".arj",
            ".lnk",
            ".lzh",
            ".ps1",
            ".scr",
            ".vbs",
            ".zipx"
        };

        private static List<string> _executableExtensions = new List<string>
        {
            ".bat",
            ".cmd",
            ".exe",
            ".sh"
        };

        public static HashSet<string> ArchiveExtensions => new HashSet<string>(_archiveExtensions, StringComparer.OrdinalIgnoreCase);
        public static HashSet<string> DangerousExtensions => new HashSet<string>(_dangerousExtensions, StringComparer.OrdinalIgnoreCase);
        public static HashSet<string> ExecutableExtensions => new HashSet<string>(_executableExtensions, StringComparer.OrdinalIgnoreCase);
    }
}
