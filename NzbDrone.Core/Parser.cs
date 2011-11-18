using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
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
                                    new Regex(@"^(?<title>.+?)?\W*(?<airyear>\d{4})\W+(?<airmonth>\d{2})\W+(?<airday>\d{2})\W?(?!\\)",
                                        RegexOptions.IgnoreCase | RegexOptions.Compiled),

                                    //Multi-Part episodes without a title (S01E05.S01E06)
                                    new Regex(@"^(?:\W*S?(?<season>\d{1,2}(?!\d+))(?:(?:\-|[ex]|\s){1,2}(?<episode>\d{1,2}(?!\d+)))+){2,}\W?(?!\\)",
			                            RegexOptions.IgnoreCase | RegexOptions.Compiled),

                                    //Multi-episode (S01E05E06, S01E05-06, etc)
                                    new Regex(@"^(?<title>.+?)(?:\W+S?(?<season>\d{1,2}(?!\d+))(?:(?:\-|[ex]|\s){1,2}(?<episode>\d{1,2}(?!\d+)))+){2,}\W?(?!\\)",
		                                RegexOptions.IgnoreCase | RegexOptions.Compiled),

                                    //Single episodes (S01E05, 1x056, etc)
                                    new Regex(@"^(?<title>.+?)(?:\W+S?(?<season>\d{1,2}(?!\d+))(?:(?:\-|[ex]|\s){1,2}(?<episode>\d{1,2}(?!\d+)))+)\W?(?!\\)",
		                                RegexOptions.IgnoreCase | RegexOptions.Compiled),

                                    //No Title - Single episodes or multi-episode (S01E05E06, S01E05-06, etc)
                                    new Regex(@"^(?:\W?S?(?<season>\d{1,2}(?!\d+))(?:(?:\-|[ex]|\s){1,2}(?<episode>\d{1,2}(?!\d+)))+\W*)+\W?(?!\\)",
			                            RegexOptions.IgnoreCase | RegexOptions.Compiled),

                                    //Episodes over 99 (3-digits or more)
                                    new Regex(@"^(?<title>.*?)(?:\W?S?(?<season>\d{1,2}(?!\d+))(?:(?:\-|[ex]|\s){1,2}(?<episode>\d+))+)+\W?(?!\\)",
                                        RegexOptions.IgnoreCase | RegexOptions.Compiled),

                                    //Supports 1103/1113 naming
                                    new Regex(@"^(?<title>.+?)?(?:\W?(?<season>\d{2})(?<episode>\d{2}(?!p|i)))+\W?(?!\\)",
                                        RegexOptions.IgnoreCase | RegexOptions.Compiled),

                                    //Supports 103/113 naming
                                    new Regex(@"^(?<title>.+?)?(?:\W?(?<season>\d{1})(?<episode>\d{2}(?!p|i|\d+)))+\W?(?!\\)",
                                        RegexOptions.IgnoreCase | RegexOptions.Compiled),

                                        //Supports Season only releases
                                    new Regex(@"^(?<title>.+?)\W(?:S|Season)\W?(?<season>\d{1,2}(?!\d+))\W?(?<extras>EXTRAS|SUBPACK)?(?!\\)",
                                        RegexOptions.IgnoreCase | RegexOptions.Compiled)
                                };

        private static readonly Regex NormalizeRegex = new Regex(@"((^|\W)(a|an|the|and|or|of)($|\W))|\W|(?:(?<=[^0-9]+)|\b)(?!(?:19\d{2}|20\d{2}))\d+(?=[^0-9ip]+|\b)",
                                                                 RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex SimpleTitleRegex = new Regex(@"480[i|p]|720[i|p]|1080[i|p]|[x|h|x\s|h\s]264|DD\W?5\W1|\<|\>|\?|\*|\:|\||""",
                                                              RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ReportSizeRegex = new Regex(@"(?<value>\d+\.\d{1,2}|\d+\,\d+\.\d{1,2})\W?(?<unit>GB|MB|GiB|MiB)",
                                                              RegexOptions.IgnoreCase | RegexOptions.Compiled);


        /// <summary>
        ///   Parses a file path into list of episodes it contains
        /// </summary>
        /// <param name = "path">Path of the file to parse</param>
        /// <returns>List of episodes contained in the file</returns>
        internal static EpisodeParseResult ParsePath(string path)
        {
            var fileInfo = new FileInfo(path);
            return ParseTitle(fileInfo.Name);
        }

        /// <summary>
        ///   Parses a post title into list of episodes it contains
        /// </summary>
        /// <param name = "title">Title of the report</param>
        /// <returns>List of episodes contained in the post</returns>
        internal static EpisodeParseResult ParseTitle(string title)
        {
            Logger.Trace("Parsing string '{0}'", title);
            var simpleTitle = SimpleTitleRegex.Replace(title, String.Empty);

            foreach (var regex in ReportTitleRegex)
            {
                //Use only the filename, not the entire path
                var match = regex.Matches(simpleTitle);

                if (match.Count != 0)
                {
                    var seriesName = NormalizeTitle(match[0].Groups["title"].Value);

                    int airyear;
                    Int32.TryParse(match[0].Groups["airyear"].Value, out airyear);

                    EpisodeParseResult parsedEpisode;

                    if (airyear < 1)
                    {
                        var seasons = new List<int>();

                        foreach (Capture seasonCapture in match[0].Groups["season"].Captures)
                        {
                            int s;
                            if (Int32.TryParse(seasonCapture.Value, out s))
                                seasons.Add(s);
                        }

                        //If more than 1 season was parsed go to the next REGEX (A multi-season release is unlikely)
                        if (seasons.Distinct().Count() != 1)
                            continue;

                        var season = seasons[0];

                        parsedEpisode = new EpisodeParseResult
                        {
                            CleanTitle = seriesName,
                            SeasonNumber = season,
                            EpisodeNumbers = new List<int>()
                        };

                        foreach (Match matchGroup in match)
                        {
                            var count = matchGroup.Groups["episode"].Captures.Count;

                            //Allows use to return a list of 0 episodes (We can handle that as a full season release)
                            if (count > 0)
                            {
                                var first = Convert.ToInt32(matchGroup.Groups["episode"].Captures[0].Value);
                                var last = Convert.ToInt32(matchGroup.Groups["episode"].Captures[count - 1].Value);

                                for (int i = first; i <= last; i++)
                                {
                                    parsedEpisode.EpisodeNumbers.Add(i);
                                }
                            }

                            else
                            {
                                //Check to see if this is an "Extras" or "SUBPACK" release, if it is, return NULL
                                //Todo: Set a "Extras" flag in EpisodeParseResult if we want to download them ever
                                if (!String.IsNullOrEmpty(match[0].Groups["extras"].Value))
                                    return null;

                                parsedEpisode.FullSeason = true;
                            }
                        }
                    }

                    else
                    {
                        //Try to Parse as a daily show
                        var airmonth = Convert.ToInt32(match[0].Groups["airmonth"].Value);
                        var airday = Convert.ToInt32(match[0].Groups["airday"].Value);

                        parsedEpisode = new EpisodeParseResult
                        {
                            CleanTitle = seriesName,
                            AirDate = new DateTime(airyear, airmonth, airday),
                            Language = ParseLanguage(simpleTitle)
                        };
                    }

                    parsedEpisode.Quality = ParseQuality(title);

                    Logger.Trace("Episode Parsed. {0}", parsedEpisode);

                    return parsedEpisode;
                }
            }
            Logger.Warn("Unable to parse episode info. {0}", title);
            return null;
        }

        /// <summary>
        ///   Parses a post title to find the series that relates to it
        /// </summary>
        /// <param name = "title">Title of the report</param>
        /// <returns>Normalized Series Name</returns>
        internal static string ParseSeriesName(string title)
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

            return String.Empty;
        }

        internal static Quality ParseQuality(string name)
        {
            Logger.Trace("Trying to parse quality for {0}", name);

            name = name.Trim();
            var normalizedName = NormalizeTitle(name);
            var result = new Quality { QualityType = QualityTypes.Unknown };
            result.Proper = normalizedName.Contains("proper");

            if (normalizedName.Contains("dvd") || normalizedName.Contains("bdrip") || normalizedName.Contains("brrip"))
            {
                result.QualityType = QualityTypes.DVD;
                return result;
            }

            if (normalizedName.Contains("xvid") || normalizedName.Contains("divx"))
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
                        case ".wmv":
                        case ".mp4":
                            {
                                result.QualityType = QualityTypes.SDTV;
                                break;
                            }
                        case ".mkv":
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

            if ((normalizedName.Contains("sdtv") || (result.QualityType == QualityTypes.Unknown && normalizedName.Contains("hdtv"))) && !normalizedName.Contains("mpeg"))
            {
                result.QualityType = QualityTypes.SDTV;
                return result;
            }

            return result;
        }

        public static LanguageType ParseLanguage(string title)
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

        /// <summary>
        ///   Normalizes the title. removing all non-word characters as well as common tokens
        ///   such as 'the' and 'and'
        /// </summary>
        /// <param name = "title">title</param>
        /// <returns></returns>
        public static string NormalizeTitle(string title)
        {
            long number = 0;

            //If Title only contains numbers return it as is.
            if (Int64.TryParse(title, out number))
                return title;

            return NormalizeRegex.Replace(title, String.Empty).ToLower();
        }

        public static string NormalizePath(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path can not be null or empty");

            var info = new FileInfo(path);

            if (info.FullName.StartsWith(@"\\")) //UNC
            {
                return info.FullName.TrimEnd('/', '\\', ' ');
            }

            return info.FullName.Trim('/', '\\', ' ');
        }

        public static long GetReportSize(string sizeString)
        {
            var match = ReportSizeRegex.Matches(sizeString);

            if (match.Count != 0)
            {
                var value = Convert.ToDecimal(Regex.Replace(match[0].Groups["value"].Value, "\\,", ""));

                var unit = match[0].Groups["unit"].Value;

                if (unit.Equals("MB", StringComparison.InvariantCultureIgnoreCase) || unit.Equals("MiB", StringComparison.InvariantCultureIgnoreCase))
                    return Convert.ToInt64(value * 1048576L);

                if (unit.Equals("GB", StringComparison.InvariantCultureIgnoreCase) || unit.Equals("GiB", StringComparison.InvariantCultureIgnoreCase))
                    return Convert.ToInt64(value * 1073741824L);
            }
            return 0;
        }
    }
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
