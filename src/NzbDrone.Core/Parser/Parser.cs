using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser
{
    public static class Parser
    {
        private static readonly Logger Logger =  NzbDroneLogger.GetLogger();

        private static readonly Regex[] ReportTitleRegex = new[]
            {
                //Episodes with airdate
                new Regex(@"^(?<title>.+?)?\W*(?<airyear>\d{4})\W+(?<airmonth>[0-1][0-9])\W+(?<airday>[0-3][0-9])",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Anime - Absolute Episode Number + Title + Season+Episode
                //Todo: This currently breaks series that start with numbers
//                new Regex(@"^(?:(?<absoluteepisode>\d{2,3})(?:_|-|\s|\.)+)+(?<title>.+?)(?:\W|_)+(?:S?(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:(?:\-|[ex]|\W[ex]){1,2}(?<episode>\d{2}(?!\d+)))+)",
//                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Anime - [SubGroup] Title Absolute Episode Number + Season+Episode
                new Regex(@"^(?:\[(?<subgroup>.+?)\](?:_|-|\s|\.))(?<title>.+?)(?:(?:\W|_)+(?<absoluteepisode>\d{2,3}))+(?:_|-|\s|\.)+(?:S?(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:(?:\-|[ex]|\W[ex]){1,2}(?<episode>\d{2}(?!\d+)))+)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Anime - [SubGroup] Title Season+Episode + Absolute Episode Number
                new Regex(@"^(?:\[(?<subgroup>.+?)\](?:_|-|\s|\.))(?<title>.+?)(?:\W|_)+(?:S?(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:(?:\-|[ex]|\W[ex]){1,2}(?<episode>\d{2}(?!\d+)))+)(?:\s|\.)(?:(?<absoluteepisode>\d{2,3})(?:_|-|\s|\.|$)+)+",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Anime - [SubGroup] Title Absolute Episode Number
                new Regex(@"^\[(?<subgroup>.+?)\](?:_|-|\s|\.)?(?<title>.+?)(?:(?:\W|_)+(?<absoluteepisode>\d{2,}))+",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Multi-Part episodes without a title (S01E05.S01E06)
                new Regex(@"^(?:\W*S?(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:(?:[ex]){1,2}(?<episode>\d{1,3}(?!\d+)))+){2,}",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Multi-episode Repeated (S01E05 - S01E06, 1x05 - 1x06, etc)
                new Regex(@"^(?<title>.+?)(?:(\W|_)+S?(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:(?:[ex]){1,2}(?<episode>\d{1,3}(?!\d+)))+){2,}",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Episodes without a title, Single (S01E05, 1x05) AND Multi (S01E04E05, 1x04x05, etc)
                new Regex(@"^(?:S?(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:(?:\-|[ex]|\W[ex]|_){1,2}(?<episode>\d{2,3}(?!\d+)))+)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Episodes with a title, Single episodes (S01E05, 1x05, etc) & Multi-episode (S01E05E06, S01E05-06, S01E05 E06, etc)
                new Regex(@"^(?<title>.+?)(?:(\W|_)+S?(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:(?:\-|[ex]|\W[ex]|_){1,2}(?<episode>\d{2,3}(?!\d+)))+)\W?(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Episodes with a title, Single episodes (S01E05, 1x05, etc) & Multi-episode (S01E05E06, S01E05-06, S01E05 E06, etc)
                new Regex(@"^(?<title>.+?)(?:\W+S?(?<season>(?<!\d+)(?:\d{1,2}|\d{4})(?!\d+))(?:(?:\-|[ex]|\W[ex]){1,2}(?<episode>\d{2,3}(?!\d+)))+)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Episodes with single digit episode number (S01E1, S01E5E6, etc)
                new Regex(@"^(?<title>.*?)(?:\W?S?(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:(?:\-|[ex]){1,2}(?<episode>\d{1}))+)+(\W+|_|$)(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Anime - Title Absolute Episode Number [SubGroup] 
                new Regex(@"^(?<title>.+?)(?:(?:_|-|\s|\.)+(?<absoluteepisode>\d{2,3}))+(?:.+?)\[(?<subgroup>.+?)\](?:\.|$)",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Supports 103/113 naming
                new Regex(@"^(?<title>.+?)?(?:\W?(?<season>(?<!\d+)\d{1})(?<episode>\d{2}(?!\w|\d+)))+",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Mini-Series, treated as season 1, episodes are labelled as Part01, Part 01, Part.1
                new Regex(@"^(?<title>.+?)(?:\W+(?:(?:Part\W?|(?<!\d+\W+)e)(?<episode>\d{1,2}(?!\d+)))+)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Supports Season 01 Episode 03
                new Regex(@"(?:.*(?:\""|^))(?<title>.*?)(?:\W?Season\W?)(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:\W|_)(?:Episode\W)(?<episode>(?<!\d+)\d{1,2}(?!\d+))",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Supports Season only releases
                new Regex(@"^(?<title>.+?)\W(?:S|Season)\W?(?<season>\d{1,2}(?!\d+))(\W+|_|$)(?<extras>EXTRAS|SUBPACK)?(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Supports 1103/1113 naming
                new Regex(@"^(?<title>.+?)?(?:\W?(?<season>(?<!\d+|\(|\[|e|x)\d{2})(?<episode>(?<!e|x)\d{2}(?!p|i|\d+|\)|\]|\W\d+)))+(\W+|_|$)(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //4-digit episode number
                //Episodes without a title, Single (S01E05, 1x05) AND Multi (S01E04E05, 1x04x05, etc)
                new Regex(@"^(?:S?(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:(?:\-|[ex]|\W[ex]|_){1,2}(?<episode>\d{4}(?!\d+|i|p)))+)(\W+|_|$)(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Episodes with a title, Single episodes (S01E05, 1x05, etc) & Multi-episode (S01E05E06, S01E05-06, S01E05 E06, etc)
                new Regex(@"^(?<title>.+?)(?:(\W|_)+S?(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:(?:\-|[ex]|\W[ex]|_){1,2}(?<episode>\d{4}(?!\d+|i|p)))+)\W?(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Anime - Title Absolute Episode Number
                new Regex(@"^(?<title>.+?)(?:(?:_|-|\s|\.)+e(?<absoluteepisode>\d{2,3}))+",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled)
            };

        private static readonly Regex NormalizeRegex = new Regex(@"((^|\W|_)(a|an|the|and|or|of)($|\W|_))|\W|_|(?:(?<=[^0-9]+)|\b)(?!(?:19\d{2}|20\d{2}))\d+(?=[^0-9ip]+|\b)",
                                                                 RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex SimpleTitleRegex = new Regex(@"480[i|p]|720[i|p]|1080[i|p]|[x|h|x\s|h\s]264|DD\W?5\W1|\<|\>|\?|\*|\:|\|",
                                                                   RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex MultiPartCleanupRegex = new Regex(@"\(\d+\)$", RegexOptions.Compiled);

        private static readonly Regex LanguageRegex = new Regex(@"(?:\W|_)(?<italian>ita|italian)|(?<german>german\b)|(?<flemish>flemish)|(?<greek>greek)|(?<french>(?:\W|_)FR)(?:\W|_)",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex YearInTitleRegex = new Regex(@"^(?<title>.+?)(?:\W|_)?(?<year>\d{4})",
                                                                   RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static ParsedEpisodeInfo ParsePath(string path)
        {
            var fileInfo = new FileInfo(path);

            var result = ParseTitle(fileInfo.Name);

            if (result == null)
            {
                Logger.Trace("Attempting to parse episode info using full path. {0}", fileInfo.FullName);
                result = ParseTitle(fileInfo.FullName);
            }

            if (result == null)
            {
                Logger.Warn("Unable to parse episode info from path {0}", path);
                return null;
            }

            result.ReleaseGroup = ParseReleaseGroup(fileInfo.Name.Replace(fileInfo.Extension, ""));

            return result;
        }

        public static ParsedEpisodeInfo ParseTitle(string title)
        {
            try
            {
                if (!ValidateBeforeParsing(title)) return null;

                Logger.Trace("Parsing string '{0}'", title);
                var simpleTitle = SimpleTitleRegex.Replace(title, String.Empty);

                foreach (var regex in ReportTitleRegex)
                {
                    var regexString = regex.ToString();
                    var match = regex.Matches(simpleTitle);

                    if (match.Count != 0)
                    {
                        Debug.WriteLine(regex);
                        try
                        {
                            var result = ParseMatchCollection(match);
                            if (result != null)
                            {
                                result.Language = ParseLanguage(title);
                                Logger.Trace("Language parsed: {0}", result.Language);

                                result.Quality = QualityParser.ParseQuality(title);
                                Logger.Trace("Quality parsed: {0}", result.Quality);

                                result.ReleaseGroup = ParseReleaseGroup(title);
                                Logger.Trace("Release Group parsed: {0}", result.ReleaseGroup);

                                return result;
                            }
                        }
                        catch (InvalidDateException ex)
                        {
                            Logger.TraceException(ex.Message, ex);
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (!title.ToLower().Contains("password") && !title.ToLower().Contains("yenc"))
                    Logger.ErrorException("An error has occurred while trying to parse " + title, e);
            }

            Logger.Trace("Unable to parse {0}", title);
            return null;
        }

        public static string ParseSeriesName(string title)
        {
            Logger.Trace("Parsing string '{0}'", title);

            var parseResult = ParseTitle(title);

            if (parseResult == null)
            {
                return CleanSeriesTitle(title);
            }

            return parseResult.SeriesTitle;
        }

        public static string CleanSeriesTitle(this string title)
        {
            long number = 0;

            //If Title only contains numbers return it as is.
            if (Int64.TryParse(title, out number))
                return title;

            return NormalizeRegex.Replace(title, String.Empty).ToLower();
        }

        public static string CleanupEpisodeTitle(string title)
        {
            //this will remove (1),(2) from the end of multi part episodes.
            return MultiPartCleanupRegex.Replace(title, string.Empty).Trim();
        }

        public static string ParseReleaseGroup(string title)
        {
            const string defaultReleaseGroup = "DRONE";

            title = title.Trim();
            var index = title.LastIndexOf('-');

            if (index < 0)
                index = title.LastIndexOf(' ');

            if (index < 0)
                return defaultReleaseGroup;

            var group = title.Substring(index + 1);

            if (group.Length == title.Length)
                return String.Empty;

            group = group.Trim('-', ' ', '[', ']');

            if (group.ToLower() == "480p" ||
                group.ToLower() == "720p" ||
                group.ToLower() == "1080p")
            {
                return defaultReleaseGroup;
            }

            return group;
        }

        private static SeriesTitleInfo GetSeriesTitleInfo(string title)
        {
            var seriesTitleInfo = new SeriesTitleInfo();
            seriesTitleInfo.Title = title;

            var match = YearInTitleRegex.Match(title);

            if (!match.Success)
            {
                seriesTitleInfo.TitleWithoutYear = title;
            }

            else
            {
                seriesTitleInfo.TitleWithoutYear = match.Groups["title"].Value;
                seriesTitleInfo.Year = Convert.ToInt32(match.Groups["year"].Value);
            }

            return seriesTitleInfo;
        }

        private static ParsedEpisodeInfo ParseMatchCollection(MatchCollection matchCollection)
        {
            var seriesName = matchCollection[0].Groups["title"].Value.Replace('.', ' ');

            int airYear;
            Int32.TryParse(matchCollection[0].Groups["airyear"].Value, out airYear);

            ParsedEpisodeInfo result;

            if (airYear < 1900)
            {
                var seasons = new List<int>();

                foreach (Capture seasonCapture in matchCollection[0].Groups["season"].Captures)
                {
                    int parsedSeason;
                    if (Int32.TryParse(seasonCapture.Value, out parsedSeason))
                        seasons.Add(parsedSeason);
                }

                //If no season was found it should be treated as a mini series and season 1
                if (seasons.Count == 0)
                    seasons.Add(1);

                //If more than 1 season was parsed go to the next REGEX (A multi-season release is unlikely)
                if (seasons.Distinct().Count() > 1)
                    return null;

                result = new ParsedEpisodeInfo
                {
                    SeasonNumber = seasons.First(),
                    EpisodeNumbers = new int[0],
                    AbsoluteEpisodeNumbers = new int[0]
                };

                foreach (Match matchGroup in matchCollection)
                {
                    var episodeCaptures = matchGroup.Groups["episode"].Captures.Cast<Capture>().ToList();
                    var absoluteEpisodeCaptures = matchGroup.Groups["absoluteepisode"].Captures.Cast<Capture>().ToList();

                    //Allows use to return a list of 0 episodes (We can handle that as a full season release)
                    if (episodeCaptures.Any())
                    {
                        var first = Convert.ToInt32(episodeCaptures.First().Value);
                        var last = Convert.ToInt32(episodeCaptures.Last().Value);

                        if (first > last)
                        {
                            return null;
                        }

                        var count = last - first + 1;
                        result.EpisodeNumbers = Enumerable.Range(first, count).ToArray();
                    }

                    if (absoluteEpisodeCaptures.Any())
                    {
                        var first = Convert.ToInt32(absoluteEpisodeCaptures.First().Value);
                        var last = Convert.ToInt32(absoluteEpisodeCaptures.Last().Value);

                        if (first > last)
                        {
                            return null;
                        }

                        var count = last - first + 1;
                        result.AbsoluteEpisodeNumbers = Enumerable.Range(first, count).ToArray();
                    }

                    if (!episodeCaptures.Any() && !absoluteEpisodeCaptures.Any())
                    {
                        //Check to see if this is an "Extras" or "SUBPACK" release, if it is, return NULL
                        //Todo: Set a "Extras" flag in EpisodeParseResult if we want to download them ever
                        if (!String.IsNullOrWhiteSpace(matchCollection[0].Groups["extras"].Value))
                            return null;

                        result.FullSeason = true;
                    }
                }
                if (result.AbsoluteEpisodeNumbers.Any() && !result.EpisodeNumbers.Any())
                {
                    result.SeasonNumber = 0;
                }
            }

            else
            {
                //Try to Parse as a daily show
                var airmonth = Convert.ToInt32(matchCollection[0].Groups["airmonth"].Value);
                var airday = Convert.ToInt32(matchCollection[0].Groups["airday"].Value);

                //Swap day and month if month is bigger than 12 (scene fail)
                if (airmonth > 12)
                {
                    var tempDay = airday;
                    airday = airmonth;
                    airmonth = tempDay;
                }

                var airDate = new DateTime(airYear, airmonth, airday);

                //Check if episode is in the future (most likely a parse error)
                if (airDate > DateTime.Now.AddDays(1).Date || airDate < new DateTime(1970, 1, 1))
                {
                    throw new InvalidDateException("Invalid date found: {0}", airDate);
                }

                result = new ParsedEpisodeInfo
                {
                    AirDate = airDate.ToString(Episode.AIR_DATE_FORMAT),
                };
            }

            result.SeriesTitle = CleanSeriesTitle(seriesName);
            result.SeriesTitleInfo = GetSeriesTitleInfo(result.SeriesTitle);

            Logger.Trace("Episode Parsed. {0}", result);

            return result;
        }

        private static Language ParseLanguage(string title)
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

            if (lowerTitle.Contains("finnish"))
                return Language.Finnish;

            if (lowerTitle.Contains("turkish"))
                return Language.Turkish;

            if (lowerTitle.Contains("portuguese"))
                return Language.Portuguese;

            if (lowerTitle.Contains("nlsub"))
                return Language.Norwegian;

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

            return Language.English;
        }

        private static bool ValidateBeforeParsing(string title)
        {
            if (title.ToLower().Contains("password") && title.ToLower().Contains("yenc"))
            {
                Logger.Trace("");
                return false;
            }

            if (!title.Any(Char.IsLetterOrDigit))
            {
                return false;
            }

            return true;
        }
    }
}