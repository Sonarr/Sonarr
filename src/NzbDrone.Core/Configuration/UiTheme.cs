using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Configuration
{
    public static class UiTheme
    {
        public const string Auto = "auto";
        public const string Light = "light";
        public const string Dark = "dark";
        public const string Oled = "oled";

        private static readonly HashSet<string> SupportedThemes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Auto,
            Light,
            Dark,
            Oled
        };

        public static bool IsValid(string theme)
        {
            return !string.IsNullOrWhiteSpace(theme) && SupportedThemes.Contains(theme);
        }

        public static string Normalize(string theme)
        {
            return IsValid(theme) ? theme.ToLowerInvariant() : Auto;
        }
    }
}
