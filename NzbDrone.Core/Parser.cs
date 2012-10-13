using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Core
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
                                    new Regex(@"^(?:S?(?<season>(?<!\d+)\d{1,2}(?!\d+))(?:(?:\-|[ex]|\W[ex])(?<episode>\d{2}(?!\d+)))+\W*)+\W?(?!\\)",
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

                                    //Mini-Series, treated as season 1, episodes are labeled as Part01, Part 01, Part.1
                                    new Regex(@"^(?<title>.+?)(?:\W+(?:(?:Part\W?|(?<!\d+\W+)e)(?<episode>\d{1,2}(?!\d+)))+)\W?(?!\\)",
                                        RegexOptions.IgnoreCase | RegexOptions.Compiled),

                                    //Supports 1103/1113 naming
                                    new Regex(@"^(?<title>.+?)?(?:\W?(?<season>(?<!\d+|\(|\[)\d{2})(?<episode>\d{2}(?!p|i|\d+|\)|\])))+\W?(?!\\)",
                                        RegexOptions.IgnoreCase | RegexOptions.Compiled),

                                        //Supports Season only releases
                                    new Regex(@"^(?<title>.+?)\W(?:S|Season)\W?(?<season>\d{1,2}(?!\d+))\W?(?<extras>EXTRAS|SUBPACK)?(?!\\)",
                                        RegexOptions.IgnoreCase | RegexOptions.Compiled)
                                };

        private static readonly Regex NormalizeRegex = new Regex(@"((^|\W)(a|an|the|and|or|of)($|\W))|\W|_|(?:(?<=[^0-9]+)|\b)(?!(?:19\d{2}|20\d{2}))\d+(?=[^0-9ip]+|\b)",
                                                                 RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex SimpleTitleRegex = new Regex(@"480[i|p]|720[i|p]|1080[i|p]|[x|h|x\s|h\s]264|DD\W?5\W1|\<|\>|\?|\*|\:|\||""",
                                                              RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ReportSizeRegex = new Regex(@"(?<value>\d+\.\d{1,2}|\d+\,\d+\.\d{1,2})\W?(?<unit>GB|MB|GiB|MiB)",
                                                              RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex HeaderRegex = new Regex(@"(?:\[.+\]\-\[.+\]\-\[.+\]\-\[)(?<nzbTitle>.+)(?:\]\-.+)",
                                                                 RegexOptions.IgnoreCase | RegexOptions.Compiled);

        internal static EpisodeParseResult ParsePath(string path)
        {
            var fileInfo = new FileInfo(path);

            var result = ParseTitle(fileInfo.Name);

            if (result == null)
            {
                Logger.Trace("Attempting to parse episode info using full path. {0}", fileInfo.FullName);
                result = ParseTitle(fileInfo.FullName);
            }

            if (result != null)
            {
                result.OriginalString = path;
            }
            else
            {
                Logger.Warn("Unable to parse episode info from path {0}", path);
            }

            return result;
        }

        internal static EpisodeParseResult ParseTitle(string title)
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
                            result.OriginalString = title;
                            result.ReleaseGroup = ParseReleaseGroup(title);
                            return result;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.ErrorException("An error has occurred while trying to parse " + title, e);
            }

            Logger.Trace("Unable to parse {0}", title);
            ReportingService.ReportParseError(title);
            return null;
        }

        private static EpisodeParseResult ParseMatchCollection(MatchCollection matchCollection)
        {
            var seriesName = matchCollection[0].Groups["title"].Value;

            int airyear;
            Int32.TryParse(matchCollection[0].Groups["airyear"].Value, out airyear);

            EpisodeParseResult parsedEpisode;

            if (airyear < 1900)
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

                parsedEpisode = new EpisodeParseResult
                {
                    SeasonNumber = seasons.First(),
                    EpisodeNumbers = new List<int>()
                };

                foreach (Match matchGroup in matchCollection)
                {
                    var episodeCaptures = matchGroup.Groups["episode"].Captures.Cast<Capture>().ToList();

                    //Allows use to return a list of 0 episodes (We can handle that as a full season release)
                    if (episodeCaptures.Any())
                    {
                        var first = Convert.ToInt32(episodeCaptures.First().Value);
                        var last = Convert.ToInt32(episodeCaptures.Last().Value);
                        parsedEpisode.EpisodeNumbers = Enumerable.Range(first, last - first + 1).ToList();
                    }
                    else
                    {
                        //Check to see if this is an "Extras" or "SUBPACK" release, if it is, return NULL
                        //Todo: Set a "Extras" flag in EpisodeParseResult if we want to download them ever
                        if (!String.IsNullOrWhiteSpace(matchCollection[0].Groups["extras"].Value))
                            return null;

                        parsedEpisode.FullSeason = true;
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

                parsedEpisode = new EpisodeParseResult
                {
                   
                    AirDate = new DateTime(airyear, airmonth, airday).Date,
                };
            }

            parsedEpisode.SeriesTitle = seriesName;

            Logger.Trace("Episode Parsed. {0}", parsedEpisode);

            return parsedEpisode;
        }

        public static string ParseSeriesName(string title)
        {
            Logger.Trace("Parsing string '{0}'", title);

            foreach (var regex in ReportTitleRegex)
            {
                var match = regex.Matches(title);

                if (match.Count != 0)
                {
                    var seriesName = NormalizeTitle(match[0].Groups["title"].Value);

                    Logger.Trace("Series Parsed. {0}", seriesName);
                    return seriesName;
                }
            }

            return NormalizeTitle(title);
        }

        internal static QualityModel ParseQuality(string name)
        {
            Logger.Trace("Trying to parse quality for {0}", name);

            name = name.Trim();
            var normalizedName = NormalizeTitle(name);
            var result = new QualityModel { QualityType = QualityTypes.Unknown };
            result.Proper = (normalizedName.Contains("proper") || normalizedName.Contains("repack"));

            if (normalizedName.Contains("dvd") || normalizedName.Contains("bdrip") || normalizedName.Contains("brrip"))
            {
                result.QualityType = QualityTypes.DVD;
                return result;
            }

            if (normalizedName.Contains("xvid") || normalizedName.Contains("divx") || normalizedName.Contains("dsr"))
            {
                if (normalizedName.Contains("bluray"))
                {
                    result.QualityType = QualityTypes.DVD;
                    return result;
                }

                result.QualityType = QualityTypes.SDTV;
                return result;
            }

            if (normalizedName.Contains("bluray"))
            {
                if (normalizedName.Contains("720p"))
                {
                    result.QualityType = QualityTypes.Bluray720p;
                    return result;
                }

                if (normalizedName.Contains("1080p"))
                {
                    result.QualityType = QualityTypes.Bluray1080p;
                    return result;
                }

                result.QualityType = QualityTypes.Bluray720p;
                return result;
            }
            if (normalizedName.Contains("webdl"))
            {
                result.QualityType = QualityTypes.WEBDL;
                return result;
            }
            if (normalizedName.Contains("x264") || normalizedName.Contains("h264") || normalizedName.Contains("720p"))
            {
                result.QualityType = QualityTypes.HDTV;
                return result;
            }
            //Based on extension

            if (result.QualityType == QualityTypes.Unknown)
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
                                result.QualityType = QualityTypes.SDTV;
                                break;
                            }
                        case ".mkv":
                        case ".ts":
                            {
                                result.QualityType = QualityTypes.HDTV;
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
                result.QualityType = QualityTypes.HDTV;
                return result;
            }

            if ((normalizedName.Contains("sdtv") || normalizedName.Contains("pdtv") ||
                (result.QualityType == QualityTypes.Unknown && normalizedName.Contains("hdtv"))) &&
                !normalizedName.Contains("mpeg"))
            {
                result.QualityType = QualityTypes.SDTV;
                return result;
            }

            return result;
        }

        internal static LanguageType ParseLanguage(string title)
        {
            var lowerTitle = title.ToLower();

            if (lowerTitle.Contains("english"))
                return LanguageType.English;

            if (lowerTitle.Contains("french"))
                return LanguageType.French;

            if (lowerTitle.Contains("spanish"))
                return LanguageType.Spanish;

            if (lowerTitle.Contains("german"))
            {
                //Make sure it doesn't contain Germany (Since we're not using REGEX for all this)
                if (!lowerTitle.Contains("germany"))
                    return LanguageType.German;
            }

            if (lowerTitle.Contains("italian"))
                return LanguageType.Italian;

            if (lowerTitle.Contains("danish"))
                return LanguageType.Danish;

            if (lowerTitle.Contains("dutch"))
                return LanguageType.Dutch;

            if (lowerTitle.Contains("japanese"))
                return LanguageType.Japanese;

            if (lowerTitle.Contains("cantonese"))
                return LanguageType.Cantonese;

            if (lowerTitle.Contains("mandarin"))
                return LanguageType.Mandarin;

            if (lowerTitle.Contains("korean"))
                return LanguageType.Korean;

            if (lowerTitle.Contains("russian"))
                return LanguageType.Russian;

            if (lowerTitle.Contains("polish"))
                return LanguageType.Polish;

            if (lowerTitle.Contains("vietnamese"))
                return LanguageType.Vietnamese;

            if (lowerTitle.Contains("swedish"))
                return LanguageType.Swedish;

            if (lowerTitle.Contains("norwegian"))
                return LanguageType.Norwegian;

            if (lowerTitle.Contains("finnish"))
                return LanguageType.Finnish;

            if (lowerTitle.Contains("turkish"))
                return LanguageType.Turkish;

            if (lowerTitle.Contains("portuguese"))
                return LanguageType.Portuguese;

            return LanguageType.English;
        }

        internal static string ParseReleaseGroup(string title)
        {
            Logger.Trace("Trying to parse release group for {0}", title);

            title = title.Trim();
            var index = title.LastIndexOf('-');
            
            if (index < 0)
                index = title.LastIndexOf(' ');

            if (index < 0)
                return String.Empty;

            var group = title.Substring(index + 1);

            if (group.Length == title.Length)
                return String.Empty;

            Logger.Trace("Release Group found: {0}", group);
            return group;
        }

        public static string NormalizeTitle(string title)
        {
            long number = 0;

            //If Title only contains numbers return it as is.
            if (Int64.TryParse(title, out number))
                return title;

            return NormalizeRegex.Replace(title, String.Empty).ToLower();
        }

        public static long GetReportSize(string sizeString)
        {
            var match = ReportSizeRegex.Matches(sizeString);

            if (match.Count != 0)
            {
                var cultureInfo = new CultureInfo("en-US");
                var value = Decimal.Parse(Regex.Replace(match[0].Groups["value"].Value, "\\,", ""), cultureInfo);

                var unit = match[0].Groups["unit"].Value;

                if (unit.Equals("MB", StringComparison.InvariantCultureIgnoreCase) || unit.Equals("MiB", StringComparison.InvariantCultureIgnoreCase))
                    return Convert.ToInt64(value * 1048576L);

                if (unit.Equals("GB", StringComparison.InvariantCultureIgnoreCase) || unit.Equals("GiB", StringComparison.InvariantCultureIgnoreCase))
                    return Convert.ToInt64(value * 1073741824L);
            }
            return 0;
        }

        internal static string ParseHeader(string header)
        {
            var match = HeaderRegex.Matches(header);

            if (match.Count != 0)
                return match[0].Groups["nzbTitle"].Value;

            return header;
        }
    }
}