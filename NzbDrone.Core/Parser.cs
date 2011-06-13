using System;
using System.Collections.Generic;
using System.IO;
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
                                    new Regex(@"^(?:\W*S?(?<season>\d{1,2}(?!\d+))(?:(?:\-|\.|[ex]|\s|\sto\s){1,2}(?<episode>\d{1,2}(?!\d+)))+){2,}\W?(?!\\)",
			                            RegexOptions.IgnoreCase | RegexOptions.Compiled),

                                    //Single episodes or multi-episode (S01E05E06, S01E05-06, etc)
                                    new Regex(@"^(?<title>.+?)(?:\W+S?(?<season>\d{1,2}(?!\d+))(?:(?:\-|\.|[ex]|\s|\sto\s){1,2}(?<episode>\d{1,2}(?!\d+)))+)+\W?(?!\\)",
		                                RegexOptions.IgnoreCase | RegexOptions.Compiled),

                                    //No Title - Single episodes or multi-episode (S01E05E06, S01E05-06, etc)
                                    new Regex(@"^(?:\W?S?(?<season>\d{1,2}(?!\d+))(?:(?:\-|\.|[ex]|\s|\sto\s){1,2}(?<episode>\d{1,2}(?!\d+)))+\W*)+\W?(?!\\)",
			                            RegexOptions.IgnoreCase | RegexOptions.Compiled),

                                    //Supports 103/113 naming
                                    new Regex(@"^(?<title>.+?)?\W?(?:\W(?<season>\d+)(?<episode>\d{2}))+\W?(?!\\)",
                                        RegexOptions.IgnoreCase | RegexOptions.Compiled),

                                    //Episodes over 99 (3-digits or more)
                                    new Regex(@"^(?<title>.*?)(?:\W?S?(?<season>\d{1,2}(?!\d+))(?:(?:\-|\.|[ex]|\s|to)+(?<episode>\d+))+)+\W?(?!\\)",
                                        RegexOptions.IgnoreCase | RegexOptions.Compiled),

                                    //Supports Season only releases
                                    new Regex(@"^(?<title>.*?)\W(?:S|Season\W?)?(?<season>\d{1,2}(?!\d+))+\W?(?!\\)",
                                        RegexOptions.IgnoreCase | RegexOptions.Compiled)
                                };

        private static readonly Regex NormalizeRegex = new Regex(@"((^|\W)(a|an|the|and|or|of)($|\W))|\W|(?:(?<=[^0-9]+)|\b)(?!(?:19\d{2}|20\d{2}))\d+(?=[^0-9ip]+|\b)",
                                                                 RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex SimpleTitleRegex = new Regex(@"480[i|p]|720[i|p]|1080[i|p]|[x|h]264|\\|\/|\<|\>|\?|\*|\:|\|",
                                                              RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        ///   Parses a post title into list of episodes it contains
        /// </summary>
        /// <param name = "title">Title of the report</param>
        /// <returns>List of episodes contained to the post</returns>
        internal static EpisodeParseResult ParseEpisodeInfo(string title)
        {
            Logger.Trace("Parsing string '{0}'", title);
            var simpleTitle = SimpleTitleRegex.Replace(title, String.Empty);

            foreach (var regex in ReportTitleRegex)
            {
                //Use only the filename, not the entire path
                var match = regex.Matches(new FileInfo(simpleTitle).Name);

                if (match.Count != 0)
                {
                    var seriesName = NormalizeTitle(match[0].Groups["title"].Value);

                    int airyear;
                    Int32.TryParse(match[0].Groups["airyear"].Value, out airyear);

                    EpisodeParseResult parsedEpisode;

                    if (airyear < 1)
                    {
                        int season;
                        Int32.TryParse(match[0].Groups["season"].Value, out season);

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
                        }
                    }

                    else
                    {
                        //Try to Parse as a daily show
                        if (airyear > 0)
                        {
                            var airmonth = Convert.ToInt32(match[0].Groups["airmonth"].Value);
                            var airday = Convert.ToInt32(match[0].Groups["airday"].Value);

                            parsedEpisode = new EpisodeParseResult
                            {
                                CleanTitle = seriesName,
                                AirDate = new DateTime(airyear, airmonth, airday),
                                Language = ParseLanguage(simpleTitle)
                            };
                        }

                        //Something went wrong with this one... return null
                        else
                            return null;
                    }

                    parsedEpisode.Quality = ParseQuality(title);

                    Logger.Trace("Episode Parsed. {0}", parsedEpisode);

                    return parsedEpisode;
                }
            }
            Logger.Warn("Unable to parse text into episode info. {0}", title);
            return null;
        }

        /// <summary>
        ///   Parses a post title into season it contains
        /// </summary>
        /// <param name = "title">Title of the report</param>
        /// <returns>Season information contained in the post</returns>
        internal static SeasonParseResult ParseSeasonInfo(string title)
        {
            Logger.Trace("Parsing string '{0}'", title);

            foreach (var regex in ReportTitleRegex)
            {
                var match = regex.Matches(title);

                if (match.Count != 0)
                {
                    var seriesName = NormalizeTitle(match[0].Groups["title"].Value);
                    int year;
                    Int32.TryParse(match[0].Groups["year"].Value, out year);

                    if (year < 1900 || year > DateTime.Now.Year + 1)
                    {
                        year = 0;
                    }

                    var seasonNumber = Convert.ToInt32(match[0].Groups["season"].Value);

                    var result = new SeasonParseResult
                                     {
                                         SeriesTitle = seriesName,
                                         SeasonNumber = seasonNumber,
                                         Year = year,
                                         Quality = ParseQuality(title)
                                     };


                    Logger.Trace("Season Parsed. {0}", result);
                    return result;
                }
            }

            return null; //Return null
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

        /// <summary>
        ///   Parses proper status out of a report title
        /// </summary>
        /// <param name = "title">Title of the report</param>
        /// <returns></returns>
        internal static bool ParseProper(string title)
        {
            return title.ToLower().Contains("proper");
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



            if (result.QualityType  == QualityTypes.Unknown)
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

            if (normalizedName.Contains("sdtv") || (result.QualityType == QualityTypes.Unknown && normalizedName.Contains("hdtv")))
            {
                result.QualityType = QualityTypes.SDTV;
                return result;
            }

            Logger.Trace("Quality Parsed:{0} Title:", result, name);
            return result;
        }



        internal static LanguageType ParseLanguage(string title)
        {
            if (title.ToLower().Contains("english"))
                return LanguageType.English;

            if (title.ToLower().Contains("french"))
                return LanguageType.French;

            if (title.ToLower().Contains("spanish"))
                return LanguageType.Spanish;

            if (title.ToLower().Contains("german"))
            {
                //Make sure it doesn't contain Germany (Since we're not using REGEX for all this)
                if (!title.ToLower().Contains("germany"))
                    return LanguageType.German;
            }

            if (title.ToLower().Contains("italian"))
                return LanguageType.Italian;

            if (title.ToLower().Contains("danish"))
                return LanguageType.Danish;

            if (title.ToLower().Contains("dutch"))
                return LanguageType.Dutch;

            if (title.ToLower().Contains("japanese"))
                return LanguageType.Japanese;

            if (title.ToLower().Contains("cantonese"))
                return LanguageType.Cantonese;

            if (title.ToLower().Contains("mandarin"))
                return LanguageType.Mandarin;

            if (title.ToLower().Contains("korean"))
                return LanguageType.Korean;

            if (title.ToLower().Contains("russian"))
                return LanguageType.Russian;

            if (title.ToLower().Contains("polish"))
                return LanguageType.Polish;

            if (title.ToLower().Contains("vietnamese"))
                return LanguageType.Vietnamese;

            if (title.ToLower().Contains("swedish"))
                return LanguageType.Swedish;

            if (title.ToLower().Contains("norwegian"))
                return LanguageType.Norwegian;

            if (title.ToLower().Contains("finnish"))
                return LanguageType.Finnish;

            if (title.ToLower().Contains("turkish"))
                return LanguageType.Turkish;

            if (title.ToLower().Contains("portuguese"))
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
    }
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
