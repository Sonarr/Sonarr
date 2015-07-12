using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Parser
{
    public static class LanguageParser
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(LanguageParser));

        private static readonly Regex LanguageRegex = new Regex(@"(?:\W|_)(?<italian>\b(?:ita|italian)\b)|(?<german>german\b|videomann)|(?<flemish>flemish)|(?<greek>greek)|(?<french>(?:\W|_)(?:FR|VOSTFR)(?:\W|_))|(?<russian>\brus\b)|(?<dutch>nl\W?subs?)|(?<hungarian>\b(?:HUNDUB|HUN)\b)|(?<hebrew>\bHebDub\b)",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex CaseSensitiveLanguageRegex = new Regex(@"(?<lithuanian>\bLT\b)|(?<czech>\bCZ\b)",
                                                                RegexOptions.Compiled);


        private static readonly Regex SubtitleLanguageRegex = new Regex(".+?[-_. ](?<iso_code>[a-z]{2,3})$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Language ParseLanguage(string title)
        {
            var lowerTitle = title.ToLower();

            if (lowerTitle.Contains("english"))
                return Language.English;

            if (lowerTitle.Contains("french"))
                return Language.French;

            if (lowerTitle.Contains("spanish"))
                return Language.Spanish;

            if (lowerTitle.Contains("danish"))
                return Language.Danish;

            if (lowerTitle.Contains("dutch"))
                return Language.Dutch;

            if (lowerTitle.Contains("japanese"))
                return Language.Japanese;

            if (lowerTitle.Contains("cantonese"))
                return Language.Cantonese;

            if (lowerTitle.Contains("mandarin"))
                return Language.Mandarin;

            if (lowerTitle.Contains("korean"))
                return Language.Korean;

            if (lowerTitle.Contains("russian"))
                return Language.Russian;

            if (lowerTitle.Contains("polish"))
                return Language.Polish;

            if (lowerTitle.Contains("vietnamese"))
                return Language.Vietnamese;

            if (lowerTitle.Contains("swedish"))
                return Language.Swedish;

            if (lowerTitle.Contains("norwegian"))
                return Language.Norwegian;

            if (lowerTitle.Contains("nordic"))
                return Language.Norwegian;

            if (lowerTitle.Contains("finnish"))
                return Language.Finnish;

            if (lowerTitle.Contains("turkish"))
                return Language.Turkish;

            if (lowerTitle.Contains("portuguese"))
                return Language.Portuguese;

            if (lowerTitle.Contains("hungarian"))
                return Language.Hungarian;

            if (lowerTitle.Contains("hebrew"))
                return Language.Hebrew;

            var regexLanguage = RegexLanguage(title);

            if (regexLanguage != Language.Unknown)
            {
                return regexLanguage;
            }

            return Language.English;
        }

        public static Language ParseSubtitleLanguage(string fileName)
        {
            try
            {
                Logger.Debug("Parsing language from subtitle file: {0}", fileName);

                var simpleFilename = Path.GetFileNameWithoutExtension(fileName);
                var languageMatch = SubtitleLanguageRegex.Match(simpleFilename);

                if (languageMatch.Success)
                {
                    var isoCode = languageMatch.Groups["iso_code"].Value;
                    var isoLanguage = IsoLanguages.Find(isoCode);

                    return isoLanguage?.Language ?? Language.Unknown;
                }

                foreach (Language language in Enum.GetValues(typeof(Language)))
                {
                    if (simpleFilename.EndsWith(language.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        return language;
                    }
                }

                Logger.Debug("Unable to parse language from subtitle file: {0}", fileName);
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Failed parsing language from subtitle file: {0}", fileName);
            }

            return Language.Unknown;
        }

        private static Language RegexLanguage(string title)
        {
            // Case sensitive
            var caseSensitiveMatch = CaseSensitiveLanguageRegex.Match(title);

            if (caseSensitiveMatch.Groups["lithuanian"].Captures.Cast<Capture>().Any())
                return Language.Lithuanian;

            if (caseSensitiveMatch.Groups["czech"].Captures.Cast<Capture>().Any())
                return Language.Czech;

            // Case insensitive
            var match = LanguageRegex.Match(title);

            if (match.Groups["italian"].Captures.Cast<Capture>().Any())
                return Language.Italian;

            if (match.Groups["german"].Captures.Cast<Capture>().Any())
                return Language.German;

            if (match.Groups["flemish"].Captures.Cast<Capture>().Any())
                return Language.Flemish;

            if (match.Groups["greek"].Captures.Cast<Capture>().Any())
                return Language.Greek;

            if (match.Groups["french"].Success)
                return Language.French;

            if (match.Groups["russian"].Success)
                return Language.Russian;

            if (match.Groups["dutch"].Success)
                return Language.Dutch;

            if (match.Groups["hungarian"].Success)
                return Language.Hungarian;

            if (match.Groups["hebrew"].Success)
                return Language.Hebrew;

            return Language.Unknown;
        }
    }
}
