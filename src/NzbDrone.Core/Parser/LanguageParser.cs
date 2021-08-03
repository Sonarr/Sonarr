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

        private static readonly RegexReplace[] CleanSeriesTitleRegex = new[]
            {
                new RegexReplace(@".*?[_. ](S\d{2}(?:E\d{2,4})*[_. ].*)", "$1", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };

        private static readonly Regex LanguageRegex = new Regex(@"(?:\W|_)(?<italian>\b(?:ita|italian)\b)|(?<german>german\b|videomann|ger[. ]dub)|(?<flemish>flemish)|(?<greek>greek)|(?<french>(?:\W|_)(?:FR|VF|VF2|VFF|VFQ|TRUEFRENCH)(?:\W|_))|(?<russian>\brus\b)|(?<hungarian>\b(?:HUNDUB|HUN)\b)|(?<hebrew>\bHebDub\b)|(?<polish>\b(?:PL\W?DUB|DUB\W?PL|LEK\W?PL|PL\W?LEK)\b)|(?<chinese>\[(?:CH[ST]|BIG5|GB)\]|简|繁|字幕)|(?<bulgarian>\bbgaudio\b)|(?<spanish>\b(?:español|castellano)\b)|(?<ukrainian>\b(?:ukr)\b)",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex CaseSensitiveLanguageRegex = new Regex(@"(?:(?i)(?<!SUB[\W|_|^]))(?:(?<lithuanian>\bLT\b)|(?<czech>\bCZ\b)|(?<polish>\bPL\b)|(?<bulgarian>\bBG\b))(?:(?i)(?![\W|_|^]SUB))",
                                                                RegexOptions.Compiled);

        private static readonly Regex SubtitleLanguageRegex = new Regex(".+?[-_. ](?<iso_code>[a-z]{2,3})([-_. ](?<tags>full|forced|foreign|default|cc|psdh|sdh))*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Language ParseLanguage(string title, bool defaultToEnglish = true)
        {
            foreach (var regex in CleanSeriesTitleRegex)
            {
                if (regex.TryReplace(ref title))
                {
                    break;
                }
            }

            var lowerTitle = title.ToLower();

            if (lowerTitle.Contains("french"))
            {
                return Language.French;
            }

            if (lowerTitle.Contains("spanish"))
            {
                return Language.Spanish;
            }

            if (lowerTitle.Contains("danish"))
            {
                return Language.Danish;
            }

            if (lowerTitle.Contains("dutch"))
            {
                return Language.Dutch;
            }

            if (lowerTitle.Contains("japanese"))
            {
                return Language.Japanese;
            }

            if (lowerTitle.Contains("icelandic"))
            {
                return Language.Icelandic;
            }

            if (lowerTitle.Contains("mandarin") || lowerTitle.Contains("cantonese") || lowerTitle.Contains("chinese"))
            {
                return Language.Chinese;
            }

            if (lowerTitle.Contains("korean"))
            {
                return Language.Korean;
            }

            if (lowerTitle.Contains("russian"))
            {
                return Language.Russian;
            }

            if (lowerTitle.Contains("polish"))
            {
                return Language.Polish;
            }

            if (lowerTitle.Contains("vietnamese"))
            {
                return Language.Vietnamese;
            }

            if (lowerTitle.Contains("swedish"))
            {
                return Language.Swedish;
            }

            if (lowerTitle.Contains("norwegian"))
            {
                return Language.Norwegian;
            }

            if (lowerTitle.Contains("finnish"))
            {
                return Language.Finnish;
            }

            if (lowerTitle.Contains("turkish"))
            {
                return Language.Turkish;
            }

            if (lowerTitle.Contains("portuguese"))
            {
                return Language.Portuguese;
            }

            if (lowerTitle.Contains("hungarian"))
            {
                return Language.Hungarian;
            }

            if (lowerTitle.Contains("hebrew"))
            {
                return Language.Hebrew;
            }

            if (lowerTitle.Contains("arabic"))
            {
                return Language.Arabic;
            }

            if (lowerTitle.Contains("hindi"))
            {
                return Language.Hindi;
            }

            if (lowerTitle.Contains("malayalam"))
            {
                return Language.Malayalam;
            }

            if (lowerTitle.Contains("ukrainian"))
            {
                return Language.Ukrainian;
            }

            if (lowerTitle.Contains("bulgarian"))
            {
                return Language.Bulgarian;
            }

            var regexLanguage = RegexLanguage(title);

            if (regexLanguage != Language.Unknown)
            {
                return regexLanguage;
            }

            if (lowerTitle.Contains("english"))
            {
                return Language.English;
            }

            return defaultToEnglish ? Language.English : Language.Unknown;
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
                    var isoLanguage = IsoLanguages.Find(isoCode.ToLower());

                    return isoLanguage?.Language ?? Language.Unknown;
                }

                foreach (Language language in Language.All)
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
            {
                return Language.Lithuanian;
            }

            if (caseSensitiveMatch.Groups["czech"].Captures.Cast<Capture>().Any())
            {
                return Language.Czech;
            }

            if (caseSensitiveMatch.Groups["polish"].Captures.Cast<Capture>().Any())
            {
                return Language.Polish;
            }

            if (caseSensitiveMatch.Groups["bulgarian"].Captures.Cast<Capture>().Any())
            {
                return Language.Bulgarian;
            }

            // Case insensitive
            var match = LanguageRegex.Match(title);

            if (match.Groups["italian"].Captures.Cast<Capture>().Any())
            {
                return Language.Italian;
            }

            if (match.Groups["german"].Captures.Cast<Capture>().Any())
            {
                return Language.German;
            }

            if (match.Groups["flemish"].Captures.Cast<Capture>().Any())
            {
                return Language.Flemish;
            }

            if (match.Groups["greek"].Captures.Cast<Capture>().Any())
            {
                return Language.Greek;
            }

            if (match.Groups["french"].Success)
            {
                return Language.French;
            }

            if (match.Groups["russian"].Success)
            {
                return Language.Russian;
            }

            if (match.Groups["dutch"].Success)
            {
                return Language.Dutch;
            }

            if (match.Groups["hungarian"].Success)
            {
                return Language.Hungarian;
            }

            if (match.Groups["hebrew"].Success)
            {
                return Language.Hebrew;
            }

            if (match.Groups["polish"].Success)
            {
                return Language.Polish;
            }

            if (match.Groups["chinese"].Success)
            {
                return Language.Chinese;
            }

            if (match.Groups["bulgarian"].Success)
            {
                return Language.Bulgarian;
            }

            if (match.Groups["ukrainian"].Success)
            {
                return Language.Ukrainian;
            }

            if (match.Groups["spanish"].Success)
            {
                return Language.Spanish;
            }

            return Language.Unknown;
        }
    }
}
