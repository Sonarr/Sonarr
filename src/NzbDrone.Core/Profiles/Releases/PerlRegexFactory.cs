using System;
using System.Text.RegularExpressions;

namespace NzbDrone.Core.Profiles.Releases
{
    public static class PerlRegexFactory
    {
        private static Regex _perlRegexFormat = new Regex(@"/(?<pattern>.*)/(?<modifiers>[a-z]*)", RegexOptions.Compiled);

        public static bool TryCreateRegex(string pattern, out Regex regex)
        {
            var match = _perlRegexFormat.Match(pattern);

            if (!match.Success)
            {
                regex = null;
                return false;
            }

            regex = CreateRegex(match.Groups["pattern"].Value, match.Groups["modifiers"].Value);
            return true;
        }

        public static Regex CreateRegex(string pattern, string modifiers)
        {
            var options = GetOptions(modifiers);

            // For now we simply expect the pattern to be .net compliant. We should probably check and reject perl-specific constructs.
            return new Regex(pattern, options | RegexOptions.Compiled);
        }

        private static RegexOptions GetOptions(string modifiers)
        {
            var options = RegexOptions.None;

            foreach (var modifier in modifiers)
            {
                switch (modifier)
                {
                    case 'm':
                        options |= RegexOptions.Multiline;
                        break;

                    case 's':
                        options |= RegexOptions.Singleline;
                        break;

                    case 'i':
                        options |= RegexOptions.IgnoreCase;
                        break;

                    case 'x':
                        options |= RegexOptions.IgnorePatternWhitespace;
                        break;

                    case 'n':
                        options |= RegexOptions.ExplicitCapture;
                        break;

                    default:
                        throw new ArgumentException("Unknown or unsupported perl regex modifier: " + modifier);
                }
            }

            return options;
        }
    }
}
