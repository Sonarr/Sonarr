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
                                                                   new Regex(
                                                                       @"(?<title>.+?)?\W?(?<year>\d{4}?)?(?:\WS?(?<season>\d{1,2})(?:(?:\-|\.|[ex]|\s|to)+(?<episode>\d+))+)+\W?(?!\\)",
                                                                       RegexOptions.IgnoreCase | RegexOptions.Compiled),
                                                                   new Regex(
                                                                       @"(?<title>.+?)?\W?(?<year>\d{4}?)?(?:\W(?<season>\d+)(?<episode>\d{2}))+\W?(?!\\)",
                                                                       RegexOptions.IgnoreCase | RegexOptions.Compiled)
                                                                   //Supports 103/113 naming
                                                               };

        private static readonly Regex[] SeasonReportTitleRegex = new[]
                                                                     {
                                                                         new Regex(
                                                                             @"(?<title>.+?)?\W?(?<year>\d{4}?)?\W(?:S|Season)?\W?(?<season>\d+)(?!\\)",
                                                                             RegexOptions.IgnoreCase |
                                                                             RegexOptions.Compiled),
                                                                     };

        private static readonly Regex NormalizeRegex = new Regex(@"((\s|^)the(\s|$))|((\s|^)and(\s|$))|[^a-z]",
                                                                 RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        ///   Parses a post title into list of episodes it contains
        /// </summary>
        /// <param name = "title">Title of the report</param>
        /// <returns>List of episodes contained to the post</returns>
        internal static EpisodeParseResult ParseEpisodeInfo(string title)
        {
            Logger.Trace("Parsing string '{0}'", title);

            foreach (var regex in ReportTitleRegex)
            {
                var simpleTitle = Regex.Replace(title, @"480[i|p]|720[i|p]|1080[i|p]|[x|h]264", String.Empty, RegexOptions.IgnoreCase | RegexOptions.Compiled);

                var match = regex.Matches(simpleTitle);

                if (match.Count != 0)
                {
                    var seriesName = NormalizeTitle(match[0].Groups["title"].Value);
                    var year = 0;
                    Int32.TryParse(match[0].Groups["year"].Value, out year);

                    if (year < 1900 || year > DateTime.Now.Year + 1)
                    {
                        year = 0;
                    }

                    var parsedEpisode = new EpisodeParseResult
                                            {
                                                Proper = title.ToLower().Contains("proper"),
                                                SeriesTitle = seriesName,
                                                SeasonNumber = Convert.ToInt32(match[0].Groups["season"].Value),
                                                Year = year,
                                                Episodes = new List<int>()
                                            };

                    foreach (Match matchGroup in match)
                    {
                        var count = matchGroup.Groups["episode"].Captures.Count;
                        var first = Convert.ToInt32(matchGroup.Groups["episode"].Captures[0].Value);
                        var last = Convert.ToInt32(matchGroup.Groups["episode"].Captures[count - 1].Value);

                        for (int i = first; i <= last; i++)
                        {
                            parsedEpisode.Episodes.Add(i);
                        }

                        //else
                        //{
                        //    foreach (Capture ep in matchGroup.Groups["episode"].Captures)
                        //    {
                        //        parsedEpisode.Episodes.Add(Convert.ToInt32(ep.Value));
                        //    }
                        //}
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

        private static QualityTypes ParseQuality(string name)
        {
            Logger.Trace("Trying to parse quality for {0}", name);

            var result = QualityTypes.Unknown;
            name = name.ToLowerInvariant();

            if (name.Contains("dvd"))
                return QualityTypes.DVD;

            if (name.Contains("bdrip") || name.Contains("brrip"))
            {
                return QualityTypes.BDRip;
            }

            if (name.Contains("xvid") || name.Contains("divx"))
            {
                if (name.Contains("bluray"))
                {
                    return QualityTypes.BDRip;
                }

                return QualityTypes.TV;
            }

            if (name.Contains("bluray"))
            {
                if (name.Contains("720p"))
                    return QualityTypes.Bluray720;

                if (name.Contains("1080p"))
                    return QualityTypes.Bluray1080;

                return QualityTypes.Bluray720;
            }
            if (name.Contains("web-dl"))
                return QualityTypes.WEBDL;
            if (name.Contains("x264") || name.Contains("h264") || name.Contains("720p"))
                return QualityTypes.HDTV;

            //Based on extension
            if (result == QualityTypes.Unknown)
            {
                switch (new FileInfo(name).Extension.ToLower())
                {
                    case ".avi":
                    case ".xvid":
                    case ".wmv":
                        {
                            result = QualityTypes.TV;
                            break;
                        }
                    case ".mkv":
                        {
                            result = QualityTypes.HDTV;
                            break;
                        }
                }
            }

            Logger.Trace("Quality Parsed:{0} Title:", result, name);
            return result;
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
}