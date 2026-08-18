using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.MediaFiles
{
    public static class FileExtensions
    {
        private static readonly Regex FileExtensionRegex = new(@"\.[a-z0-9]{2,4}$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly HashSet<string> UsenetExtensions = new HashSet<string>()
        {
            ".par2",
            ".nzb"
        };

        public static HashSet<string> ArchiveExtensions => new(StringComparer.OrdinalIgnoreCase)
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
        public static HashSet<string> DangerousExtensions => new(StringComparer.OrdinalIgnoreCase)
        {
            ".arj",
            ".lnk",
            ".lzh",
            ".ps1",
            ".scr",
            ".vbs",
            ".zipx"
        };
        public static HashSet<string> ExecutableExtensions => new(StringComparer.OrdinalIgnoreCase)
        {
            ".bat",
            ".cmd",
            ".exe",
            ".sh"
        };

        public static List<string> ParseExtensions(string input)
        {
            if (input.IsNullOrWhiteSpace())
            {
                return [];
            }

            return input.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
               .Select(e => e.Trim(' ', '.').Insert(0, "."))
               .ToList();
        }

        public static HashSet<string> ParseUserRejectedExtensions(string input)
        {
            return input.IsNullOrWhiteSpace()
                ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                : ParseExtensions(input).ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        public static string RemoveFileExtension(string title)
        {
            title = FileExtensionRegex.Replace(title, m =>
            {
                var extension = m.Value.ToLower();
                if (MediaFileExtensions.Extensions.Contains(extension) || UsenetExtensions.Contains(extension))
                {
                    return string.Empty;
                }

                return m.Value;
            });

            return title;
        }
    }
}
