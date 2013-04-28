using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser
{
    public static class Parser
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly Regex[] ReportTitleRegex = new[]
            {
                //Episodes with airdate
                new Regex(@"^(?<title>.+?)?\W*(?<airyear>\d{4})\W+(?<airmonth>[0-1][0-9])\W+(?<airday>[0-3][0-9])\W?(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Multi-Part episodes without a title (S01E05.S01E06)
                new Regex(@"^(?:\W*S?(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:(?:[ex]){1,2}(?<episode>\d{1,2}(?!\d+)))+){2,}\W?(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Multi-episode Repeated (S01E05 - S01E06, 1x05 - 1x06, etc)
                new Regex(@"^(?<title>.+?)(?:\W+S?(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:(?:[ex]){1,2}(?<episode>\d{1,2}(?!\d+)))+){2,}\W?(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Episodes without a title, Single (S01E05, 1x05) AND Multi (S01E04E05, 1x04x05, etc)
                new Regex(@"^(?:S?(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:(?:\-|[ex]|\W[ex]){1,2}(?<episode>\d{2}(?!\d+)))+)\W?(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Episodes with a title, Single episodes (S01E05, 1x05, etc) & Multi-episode (S01E05E06, S01E05-06, S01E05 E06, etc)
                new Regex(@"^(?<title>.+?)(?:\W+S?(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:(?:\-|[ex]|\W[ex]){1,2}(?<episode>\d{2}(?!\d+)))+)\W?(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Episodes over 99 (3-digits or more) (S01E105, S01E105E106, etc)
                new Regex(@"^(?<title>.*?)(?:\W?S?(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:(?:\-|[ex]){1,2}(?<episode>\d+))+)+\W?(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                new Regex(@"^(?:S?(?<season>(?<!\d+)(?:\d{1,2}|\d{4})(?!\d+))(?:(?:\-|[ex]|\W[ex])(?<episode>\d{2}(?!\d+)))+\W*)+\W?(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Episodes with a title, Single episodes (S01E05, 1x05, etc) & Multi-episode (S01E05E06, S01E05-06, S01E05 E06, etc)
                new Regex(@"^(?<title>.+?)(?:\W+S?(?<season>(?<!\d+)(?:\d{1,2}|\d{4})(?!\d+))(?:(?:\-|[ex]|\W[ex]){1,2}(?<episode>\d{2}(?!\d+)))+)\W?(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Supports 103/113 naming
                new Regex(@"^(?<title>.+?)?(?:\W?(?<season>(?<!\d+)\d{1})(?<episode>\d{2}(?!p|i|\d+)))+\W?(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Mini-Series, treated as season 1, episodes are labelled as Part01, Part 01, Part.1
                new Regex(@"^(?<title>.+?)(?:\W+(?:(?:Part\W?|(?<!\d+\W+)e)(?<episode>\d{1,2}(?!\d+)))+)\W?(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Supports 1103/1113 naming
                new Regex(@"^(?<title>.+?)?(?:\W?(?<season>(?<!\d+|\(|\[)\d{2})(?<episode>\d{2}(?!p|i|\d+|\)|\])))+\W?(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

                //Supports Season only releases
                new Regex(@"^(?<title>.+?)\W(?:S|Season)\W?(?<season>\d{1,2}(?!\d+))\W?(?<extras>EXTRAS|SUBPACK)?(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled)
            };

        private static readonly Regex NormalizeRegex = new Regex(@"((^|\W)(a|an|the|and|or|of)($|\W|_))|\W|_|(?:(?<=[^0-9]+)|\b)(?!(?:19\d{2}|20\d{2}))\d+(?=[^0-9ip]+|\b)",
                                                                 RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex SimpleTitleRegex = new Regex(@"480[i|p]|720[i|p]|1080[i|p]|[x|h|x\s|h\s]264|DD\W?5\W1|\<|\>|\?|\*|\:|\||""",
                                                                   RegexOptions.IgnoreCase | RegexOptions.Compiled);



        private static readonly Regex MultiPartCleanupRegex = new Regex(@"\(\d+\)$", RegexOptions.Compiled);

        private static readonly Regex LanguageRegex = new Regex(@"(?:\W|_)(?<italian>ita|italian)|(?<german>german\b)|(?<flemish>flemish)|(?<greek>greek)(?:\W|_)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
            }

            return result;
        }

        public static ParsedEpisodeInfo ParseTitle(string title)
        {
            try
            {
                Logger.Trace("Parsing string '{0}'", title);
                var simpleTitle = SimpleTitleRegex.Replace(title, String.Empty);

                foreach (var regex in ReportTitleRegex)
                {
                    var match = regex.Matches(simpleTitle);

                    if (match.Count != 0)
                    {
                        var result = ParseMatchCollection(match);
                        if (result != null)
                        {
                            //Check if episode is in the future (most likley a parse error)
                            if (result.AirDate > DateTime.Now.AddDays(1).Date)
                                break;

                            result.Language = ParseLanguage(title);
                            result.Quality = ParseQuality(title);
                            return result;
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
            ReportingService.ReportParseError(title);
            return null;
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
                    };

                foreach (Match matchGroup in matchCollection)
                {
                    var episodeCaptures = matchGroup.Groups["episode"].Captures.Cast<Capture>().ToList();

                    //Allows use to return a list of 0 episodes (We can handle that as a full season release)
                    if (episodeCaptures.Any())
                    {
                        var first = Convert.ToInt32(episodeCaptures.First().Value);
                        var last = Convert.ToInt32(episodeCaptures.Last().Value);
                        result.EpisodeNumbers = Enumerable.Range(first, last - first + 1).ToArray();
                    }
                    else
                    {
                        //Check to see if this is an "Extras" or "SUBPACK" release, if it is, return NULL
                        //Todo: Set a "Extras" flag in EpisodeParseResult if we want to download them ever
                        if (!String.IsNullOrWhiteSpace(matchCollection[0].Groups["extras"].Value))
                            return null;

                        result.FullSeason = true;
                    }
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

                result = new ParsedEpisodeInfo
                    {
                        AirDate = new DateTime(airYear, airmonth, airday).Date,
                    };
            }

            result.SeriesTitle = NormalizeTitle(seriesName);

            Logger.Trace("Episode Parsed. {0}", result);

            return result;
        }

        public static string ParseSeriesName(string title)
        {
            Logger.Trace("Parsing string '{0}'", title);

            var parseResult = ParseTitle(title);

            if (parseResult == null)
            {
                return NormalizeTitle(title);
            }

            return parseResult.SeriesTitle;
        }

        private static QualityModel ParseQuality(string name)
        {
            Logger.Trace("Trying to parse quality for {0}", name);

            name = name.Trim();
            var normalizedName = NormalizeTitle(name);
            var result = new QualityModel { Quality = Quality.Unknown };
            result.Proper = (normalizedName.Contains("proper") || normalizedName.Contains("repack"));

            if (normalizedName.Contains("dvd") || normalizedName.Contains("bdrip") || normalizedName.Contains("brrip"))
            {
                result.Quality = Quality.DVD;
                return result;
            }

            if (normalizedName.Contains("xvid") || normalizedName.Contains("divx") || normalizedName.Contains("dsr"))
            {
                if (normalizedName.Contains("bluray"))
                {
                    result.Quality = Quality.DVD;
                    return result;
                }

                result.Quality = Quality.SDTV;
                return result;
            }

            if (normalizedName.Contains("bluray"))
            {
                if (normalizedName.Contains("720p"))
                {
                    result.Quality = Quality.Bluray720p;
                    return result;
                }

                if (normalizedName.Contains("1080p"))
                {
                    result.Quality = Quality.Bluray1080p;
                    return result;
                }

                result.Quality = Quality.Bluray720p;
                return result;
            }
            if (normalizedName.Contains("webdl"))
            {
                if (normalizedName.Contains("1080p"))
                {
                    result.Quality = Quality.WEBDL1080p;
                    return result;
                }

                if (normalizedName.Contains("720p"))
                {
                    result.Quality = Quality.WEBDL720p;
                    return result;
                }

                if (name.Contains("[WEBDL]"))
                {
                    result.Quality = Quality.WEBDL720p;
                    return result;
                }

                result.Quality = Quality.WEBDL480p;
                return result;
            }

            if (normalizedName.Contains("trollhd") || normalizedName.Contains("rawhd"))
            {
                result.Quality = Quality.RAWHD;
                return result;
            }

            if (normalizedName.Contains("x264") || normalizedName.Contains("h264") || normalizedName.Contains("720p"))
            {
                if (normalizedName.Contains("1080p"))
                {
                    result.Quality = Quality.HDTV1080p;
                    return result;
                }

                result.Quality = Quality.HDTV720p;
                return result;
            }
            //Based on extension

            if (result.Quality == Quality.Unknown)
            {
                try
                {
                    switch (Path.GetExtension(name).ToLower())
                    {
                        case ".avi":
                        case ".xvid":
                        case ".divx":
                        case ".wmv":
                        case ".mp4":
                        case ".mpg":
                        case ".mpeg":
                        case ".mov":
                        case ".rm":
                        case ".rmvb":
                        case ".flv":
                        case ".dvr-ms":
                        case ".ogm":
                        case ".strm":
                            {
                                result.Quality = Quality.SDTV;
                                break;
                            }
                        case ".mkv":
                        case ".ts":
                            {
                                result.Quality = Quality.HDTV720p;
                                break;
                            }
                    }
                }
                catch (ArgumentException)
                {
                    //Swallow exception for cases where string contains illegal 
                    //path characters.
                }
            }

            if (name.Contains("[HDTV]"))
            {
                result.Quality = Quality.HDTV720p;
                return result;
            }

            if (normalizedName.Contains("hdtv") && normalizedName.Contains("1080p"))
            {
                result.Quality = Quality.HDTV1080p;
                return result;
            }

            if ((normalizedName.Contains("sdtv") || normalizedName.Contains("pdtv") ||
                 (result.Quality == Quality.Unknown && normalizedName.Contains("hdtv"))) &&
                !normalizedName.Contains("mpeg"))
            {
                result.Quality = Quality.SDTV;
                return result;
            }

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

            var match = LanguageRegex.Match(title);

            if (match.Groups["italian"].Captures.Cast<Capture>().Any())
                return Language.Italian;

            if (match.Groups["german"].Captures.Cast<Capture>().Any())
                return Language.German;

            if (match.Groups["flemish"].Captures.Cast<Capture>().Any())
                return Language.Flemish;

            if (match.Groups["greek"].Captures.Cast<Capture>().Any())
                return Language.Greek;

            return Language.English;
        }

        public static string NormalizeTitle(string title)
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

    }
}