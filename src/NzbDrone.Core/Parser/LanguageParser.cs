using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Parser
{
    public static class LanguageParser
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(LanguageParser));

        private static readonly RegexReplace[] CleanSeriesTitleRegex = new[]
            {
                new RegexReplace(@".*?[_. ](S\d{2}(?:E\d{2,4})*[_. ].*)", "$1", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };

        private static readonly Regex LanguagesOnlyRegex = new(@"(?<english>\b(?:ing|eng)\b)|(?<italian>\b(?:ita|italian)\b)|(?<german>(?:swiss)?german\b|videomann|ger[. ]dub|\bger\b)|(?<flemish>flemish)|(?<greek>greek)|(?<french>(?:_|\b)(?:FR|VF|VF2|VFF|VFI|VFQ|TRUEFRENCH|FRENCH|FRE|FRA)(?:_|\b))|(?<russian>\b(?:rus|ru)\b)|(?<hungarian>\b(?:HUNDUB|HUN)\b)|(?<hebrew>\bHebDub\b)|(?<polish>\b(?:PL\W?DUB|DUB\W?PL|LEK\W?PL|PL\W?LEK)\b)|(?<chinese>\[(?:CH[ST]|BIG5|GB)\]|简|繁|字幕|国语音轨[.+])|(?<bulgarian>\bbgaudio\b)|(?<spanish>\b(?:español|castellano|esp|spa(?!\(Latino\)))\b)|(?<ukrainian>\b(?:\dx?)?(?:ukr))|(?<thai>\b(?:THAI)\b)|(?<romanian>\b(?:RoDubbed|ROMANIAN)\b)|(?<catalan>[-,. ]cat[. ](?:DD|subs)|\b(?:catalan|catalán)\b)|(?<latvian>\b(?:lat|lav|lv)\b)|(?<turkish>\b(?:tur)\b)|(?<urdu>\burdu\b)|(?<romansh>\b(?:romansh|rumantsch|romansch)\b)|(?<georgian>\b(?:geo|ka|kat|georgian)\b)|(?<japanese>\(JA\)|JAP|JPN)|(?<portuguese>[_. ]por[_. ])|(?<original>\b(?:orig|original)\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex LanguageRegex = new(@$"(?:{LanguagesOnlyRegex})(?!(?:[-_. ](?:{LanguagesOnlyRegex}))*[-_. ]subs?)",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex CaseSensitiveLanguageRegex = new(@"(?:(?i)(?<!SUB[\W|_|^]))(?:(?<lithuanian>\bLT\b)|(?<czech>\bCZ\b)|(?<polish>\bPL\b)|(?<bulgarian>\bBG\b)|(?<slovak>\bSK\b)|(?<german>\bDE\b))(?:(?i)(?![\W|_|^]SUB))",
                                                                RegexOptions.Compiled);

        private static readonly Regex GermanDualLanguageRegex = new(@"(?<!WEB[-_. ]?)\bDL\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex GermanMultiLanguageRegex = new(@"\bML\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SubtitleLanguageRegex = new(".+?([-_. ](?<tags>forced|foreign|default|cc|psdh|sdh))*[-_. ](?<iso_code>[a-z]{2,3})([-_. ](?<tags>forced|foreign|default|cc|psdh|sdh))*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SubtitleLanguageTitleRegex = new(@".+?(\.((?<tags1>forced|foreign|default|cc|psdh|sdh)|(?<iso_code>[a-z]{2,3})))*[-_. ](?<title>[^.]*)(\.((?<tags2>forced|foreign|default|cc|psdh|sdh)|(?<iso_code>[a-z]{2,3})))*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SubtitleTitleRegex = new(@"^((?<title>.+) - )?(?<copy>(?<!\d+)\d{1,3}(?!\d+))$", RegexOptions.Compiled);

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

            if (lowerTitle.Contains("brazilian") || lowerTitle.Contains("dublado"))
            {
                languages.Add(Language.PortugueseBrazil);
            }

            if (lowerTitle.Contains("latino"))
            {
                languages.Add(Language.SpanishLatino);
            }

            if (lowerTitle.Contains("latvian"))
            {
                languages.Add(Language.Latvian);
            }

            if (lowerTitle.Contains("azerbaijani") || lowerTitle.Contains("azerbaijan"))
            {
                languages.Add(Language.Azerbaijani);
            }

            if (lowerTitle.Contains("uzbek") || lowerTitle.Contains("uzbekistan"))
            {
                languages.Add(Language.Uzbek);
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

            if (languages.Count == 1 && languages.Single() == Language.German)
            {
                if (GermanDualLanguageRegex.IsMatch(title))
                {
                    Logger.Trace("Adding original language because the release title contains German DL tag");
                    languages.Add(Language.Original);
                }
                else if (GermanMultiLanguageRegex.IsMatch(title))
                {
                    Logger.Trace("Adding original language and English because the release title contains German ML tag");
                    languages.Add(Language.Original);
                    languages.Add(Language.English);
                }
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

                foreach (var language in Language.All)
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

        public static SubtitleTitleInfo ParseBasicSubtitle(string fileName)
        {
            return new SubtitleTitleInfo
            {
                TitleFirst = false,
                LanguageTags = ParseLanguageTags(fileName),
                Language = ParseSubtitleLanguage(fileName)
            };
        }

        public static SubtitleTitleInfo ParseSubtitleLanguageInformation(string fileName)
        {
            var simpleFilename = Path.GetFileNameWithoutExtension(fileName);
            var matchTitle = SubtitleLanguageTitleRegex.Match(simpleFilename);

            if (!matchTitle.Groups["title"].Success || (matchTitle.Groups["iso_code"].Captures.Count is var languageCodeNumber && languageCodeNumber != 1))
            {
                Logger.Debug("Could not parse a title from subtitle file: {0}. Falling back to parsing without title.", fileName);

                return ParseBasicSubtitle(fileName);
            }

            var isoCode = matchTitle.Groups["iso_code"].Value;
            var isoLanguage = IsoLanguages.Find(isoCode.ToLower());

            var language = isoLanguage?.Language ?? Language.Unknown;

            var languageTags = matchTitle.Groups["tags1"].Captures
                .Union(matchTitle.Groups["tags2"].Captures)
                .Cast<Capture>()
                .Where(tag => !tag.Value.Empty())
                .Select(tag => tag.Value.ToLower());
            var rawTitle = matchTitle.Groups["title"].Value;

            var subtitleTitleInfo = new SubtitleTitleInfo
            {
                TitleFirst = matchTitle.Groups["tags1"].Captures.Empty(),
                LanguageTags = languageTags.ToList(),
                RawTitle = rawTitle,
                Language = language
            };

            UpdateTitleAndCopyFromTitle(subtitleTitleInfo);

            return subtitleTitleInfo;
        }

        public static void UpdateTitleAndCopyFromTitle(SubtitleTitleInfo subtitleTitleInfo)
        {
            if (subtitleTitleInfo.RawTitle is null)
            {
                subtitleTitleInfo.Title = null;
                subtitleTitleInfo.Copy = 0;
            }
            else if (SubtitleTitleRegex.Match(subtitleTitleInfo.RawTitle) is var match && match.Success)
            {
                subtitleTitleInfo.Title = match.Groups["title"].Success ? match.Groups["title"].ToString() : null;
                subtitleTitleInfo.Copy = int.Parse(match.Groups["copy"].ToString());
            }
            else
            {
                subtitleTitleInfo.Title = subtitleTitleInfo.RawTitle;
                subtitleTitleInfo.Copy = 0;
            }
        }

        public static List<string> ParseLanguageTags(string fileName)
        {
            try
            {
                var simpleFilename = Path.GetFileNameWithoutExtension(fileName);
                var match = SubtitleLanguageRegex.Match(simpleFilename);
                var languageTags = match.Groups["tags"].Captures
                    .Where(tag => !tag.Value.Empty())
                    .Select(tag => tag.Value.ToLower());
                return languageTags.ToList();
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Failed parsing language tags from subtitle file: {0}", fileName);
            }

            return new List<string>();
        }

        private static List<Language> RegexLanguage(string title)
        {
            var languages = new List<Language>();

            // Case sensitive
            var caseSensitiveMatch = CaseSensitiveLanguageRegex.Match(title);

            if (caseSensitiveMatch.Groups["lithuanian"].Captures.Any())
            {
                languages.Add(Language.Lithuanian);
            }

            if (caseSensitiveMatch.Groups["czech"].Captures.Any())
            {
                languages.Add(Language.Czech);
            }

            if (caseSensitiveMatch.Groups["polish"].Captures.Any())
            {
                languages.Add(Language.Polish);
            }

            if (caseSensitiveMatch.Groups["bulgarian"].Captures.Any())
            {
                languages.Add(Language.Bulgarian);
            }

            if (caseSensitiveMatch.Groups["slovak"].Captures.Any())
            {
                languages.Add(Language.Slovak);
            }

            if (caseSensitiveMatch.Groups["german"].Captures.Any())
            {
                languages.Add(Language.German);
            }

            // Case insensitive
            var matches = LanguageRegex.Matches(title);

            foreach (Match match in matches)
            {
                if (match.Groups["english"].Success)
                {
                    languages.Add(Language.English);
                }

                if (match.Groups["italian"].Captures.Any())
                {
                    languages.Add(Language.Italian);
                }

                if (match.Groups["german"].Captures.Any())
                {
                    languages.Add(Language.German);
                }

                if (match.Groups["flemish"].Captures.Any())
                {
                    languages.Add(Language.Flemish);
                }

                if (match.Groups["greek"].Captures.Any())
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

                if (match.Groups["thai"].Success)
                {
                    languages.Add(Language.Thai);
                }

                if (match.Groups["romanian"].Success)
                {
                    languages.Add(Language.Romanian);
                }

                if (match.Groups["catalan"].Success)
                {
                    languages.Add(Language.Catalan);
                }

                if (match.Groups["latvian"].Success)
                {
                    languages.Add(Language.Latvian);
                }

                if (match.Groups["turkish"].Success)
                {
                    languages.Add(Language.Turkish);
                }

                if (match.Groups["urdu"].Success)
                {
                    languages.Add(Language.Urdu);
                }

                if (match.Groups["romansh"].Success)
                {
                    languages.Add(Language.Romansh);
                }

                if (match.Groups["georgian"].Success)
                {
                    languages.Add(Language.Georgian);
                }

                if (match.Groups["japanese"].Success)
                {
                    languages.Add(Language.Japanese);
                }

                if (match.Groups["portuguese"].Success)
                {
                    languages.Add(Language.Portuguese);
                }

                if (match.Groups["original"].Success)
                {
                    languages.Add(Language.Original);
                }
            }

            return languages;
        }
    }
}
