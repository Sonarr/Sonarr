using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Extensions;
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

        private static readonly Regex CaseSensitiveLanguageRegex = new Regex(@"(?:(?i)(?<!SUB[\W|_|^]))(?:(?<lithuanian>\bLT\b)|(?<czech>\bCZ\b)|(?<polish>\bPL\b)|(?<bulgarian>\bBG\b)|(?<slovak>\bSK\b))(?:(?i)(?![\W|_|^]SUB))",
                                                                RegexOptions.Compiled);

        private static readonly Regex SubtitleLanguageRegex = new Regex(".+?[-_. ](?<iso_code>[a-z]{2,3})([-_. ](?<tags>full|forced|foreign|default|cc|psdh|sdh))*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static List<Language> ParseLanguages(string title)
        {
            foreach (var regex in CleanSeriesTitleRegex)
            {
                if (regex.TryReplace(ref title))
                {
                    break;
                }
            }

            var lowerTitle = title.ToLower();

            var languages = new List<Language>();

            if (lowerTitle.Contains("french"))
            {
                languages.Add(Language.French);
            }

            if (lowerTitle.Contains("spanish"))
            {
                languages.Add(Language.Spanish);
            }

            if (lowerTitle.Contains("danish"))
            {
                languages.Add(Language.Danish);
            }

            if (lowerTitle.Contains("dutch"))
            {
                languages.Add(Language.Dutch);
            }

            if (lowerTitle.Contains("japanese"))
            {
                languages.Add(Language.Japanese);
            }

            if (lowerTitle.Contains("icelandic"))
            {
                languages.Add(Language.Icelandic);
            }

            if (lowerTitle.Contains("mandarin") || lowerTitle.Contains("cantonese") || lowerTitle.Contains("chinese"))
            {
                languages.Add(Language.Chinese);
            }

            if (lowerTitle.Contains("korean"))
            {
                languages.Add(Language.Korean);
            }

            if (lowerTitle.Contains("russian"))
            {
                languages.Add(Language.Russian);
            }

            if (lowerTitle.Contains("polish"))
            {
                languages.Add(Language.Polish);
            }

            if (lowerTitle.Contains("vietnamese"))
            {
                languages.Add(Language.Vietnamese);
            }

            if (lowerTitle.Contains("swedish"))
            {
                languages.Add(Language.Swedish);
            }

            if (lowerTitle.Contains("norwegian"))
            {
                languages.Add(Language.Norwegian);
            }

            if (lowerTitle.Contains("finnish"))
            {
                languages.Add(Language.Finnish);
            }

            if (lowerTitle.Contains("turkish"))
            {
                languages.Add(Language.Turkish);
            }

            if (lowerTitle.Contains("portuguese"))
            {
                languages.Add(Language.Portuguese);
            }

            if (lowerTitle.Contains("hungarian"))
            {
                languages.Add(Language.Hungarian);
            }

            if (lowerTitle.Contains("hebrew"))
            {
                languages.Add(Language.Hebrew);
            }

            if (lowerTitle.Contains("arabic"))
            {
                languages.Add(Language.Arabic);
            }

            if (lowerTitle.Contains("hindi"))
            {
                languages.Add(Language.Hindi);
            }

            if (lowerTitle.Contains("malayalam"))
            {
                languages.Add(Language.Malayalam);
            }

            if (lowerTitle.Contains("ukrainian"))
            {
                languages.Add(Language.Ukrainian);
            }

            if (lowerTitle.Contains("bulgarian"))
            {
                languages.Add(Language.Bulgarian);
            }

            if (lowerTitle.Contains("slovak"))
            {
                languages.Add(Language.Slovak);
            }

            var regexLanguages = RegexLanguage(title);

            if (regexLanguages.Any())
            {
                languages.AddRange(regexLanguages);
            }

            if (lowerTitle.Contains("english"))
            {
                languages.Add(Language.English);
            }

            if (!languages.Any())
            {
                languages.Add(Language.Unknown);
            }

            return languages.DistinctBy(l => (int)l).ToList();
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

        public static IEnumerable<string> ParseLanguageTags(string fileName)
        {
            try
            {
                var simpleFilename = Path.GetFileNameWithoutExtension(fileName);
                var match = SubtitleLanguageRegex.Match(simpleFilename);
                var languageTags = match.Groups["tags"].Captures.Cast<Capture>()
                    .Where(tag => !tag.Value.Empty())
                    .Select(tag => tag.Value.ToLower());
                return languageTags;
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Failed parsing language tags from subtitle file: {0}", fileName);
            }

            return Enumerable.Empty<string>();
        }

        private static List<Language> RegexLanguage(string title)
        {
            var languages = new List<Language>();

            // Case sensitive
            var caseSensitiveMatch = CaseSensitiveLanguageRegex.Match(title);

            if (caseSensitiveMatch.Groups["lithuanian"].Captures.Cast<Capture>().Any())
            {
                languages.Add(Language.Lithuanian);
            }

            if (caseSensitiveMatch.Groups["czech"].Captures.Cast<Capture>().Any())
            {
                languages.Add(Language.Czech);
            }

            if (caseSensitiveMatch.Groups["polish"].Captures.Cast<Capture>().Any())
            {
                languages.Add(Language.Polish);
            }

            if (caseSensitiveMatch.Groups["bulgarian"].Captures.Cast<Capture>().Any())
            {
                languages.Add(Language.Bulgarian);
            }

            if (caseSensitiveMatch.Groups["slovak"].Captures.Cast<Capture>().Any())
            {
                languages.Add(Language.Slovak);
            }

            // Case insensitive
            var match = LanguageRegex.Match(title);

            if (match.Groups["italian"].Captures.Cast<Capture>().Any())
            {
                languages.Add(Language.Italian);
            }

            if (match.Groups["german"].Captures.Cast<Capture>().Any())
            {
                languages.Add(Language.German);
            }

            if (match.Groups["flemish"].Captures.Cast<Capture>().Any())
            {
                languages.Add(Language.Flemish);
            }

            if (match.Groups["greek"].Captures.Cast<Capture>().Any())
            {
                languages.Add(Language.Greek);
            }

            if (match.Groups["french"].Success)
            {
                languages.Add(Language.French);
            }

            if (match.Groups["russian"].Success)
            {
                languages.Add(Language.Russian);
            }

            if (match.Groups["dutch"].Success)
            {
                languages.Add(Language.Dutch);
            }

            if (match.Groups["hungarian"].Success)
            {
                languages.Add(Language.Hungarian);
            }

            if (match.Groups["hebrew"].Success)
            {
                languages.Add(Language.Hebrew);
            }

            if (match.Groups["polish"].Success)
            {
                languages.Add(Language.Polish);
            }

            if (match.Groups["chinese"].Success)
            {
                languages.Add(Language.Chinese);
            }

            if (match.Groups["bulgarian"].Success)
            {
                languages.Add(Language.Bulgarian);
            }

            if (match.Groups["ukrainian"].Success)
            {
                languages.Add(Language.Ukrainian);
            }

            if (match.Groups["spanish"].Success)
            {
                languages.Add(Language.Spanish);
            }

            return languages;
        }
    }
}
