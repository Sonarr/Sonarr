using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository.Quality;
using Rss;

namespace NzbDrone.Core
{
    internal static class Parser
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly Regex[] ReportTitleRegex = new[]
                                                       {
                                                         new Regex(@"(?<title>.+?)?\W?(?<year>\d+?)?\WS?(?<season>\d+)(?:\-|\.|[a-z])(?<episode>\d+)\W(?!\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                                                         new Regex(@"(?<title>.+?)?\W?(?<year>\d+?)?\WS?(?<season>\d+)\w(?<episode>\d+)\W(?!\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled) //Supports 103 naming
                                                       };

        private static readonly Regex NormalizeRegex = new Regex(@"((\s|^)the(\s|$))|((\s|^)and(\s|$))|[^a-z]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Parses a post title into list of episodes it contains
        /// </summary>
        /// <param name="title">Title of the report</param>
        /// <returns>List of episodes contained to the post</returns>
        internal static List<EpisodeParseResult> ParseEpisodeInfo(string title)
        {
            Logger.Trace("Parsing string '{0}'", title);

            var result = new List<EpisodeParseResult>();

            foreach (var regex in ReportTitleRegex)
            {
                var match = regex.Matches(title);

                if (match.Count != 0)
                {
                    var seriesName = NormalizeTitle(match[0].Groups["title"].Value);
                    var year = 0;
                    Int32.TryParse(match[0].Groups["year"].Value, out year);

                    if (year < 1900 || year > DateTime.Now.Year + 1)
                    {
                        year = 0;
                    }

                    foreach (Match matchGroup in match)
                    {

                        var parsedEpisode = new EpisodeParseResult
                                                {
                                                    SeriesTitle = seriesName,
                                                    SeasonNumber = Convert.ToInt32(matchGroup.Groups["season"].Value),
                                                    EpisodeNumber = Convert.ToInt32(matchGroup.Groups["episode"].Value),
                                                    Year = year
                                                };


                        result.Add(parsedEpisode);

                        Logger.Trace("Episode Parsed. {0}", parsedEpisode);
                    }
                    break; //Break out of the for loop, we don't want to process every REGEX for each item otherwise we'll get duplicates
                }
            }

            return result;
        }

        /// <summary>
        /// Parses a post title to find the series that relates to it
        /// </summary>
        /// <param name="title">Title of the report</param>
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
                    var year = 0;
                    Int32.TryParse(match[0].Groups["year"].Value, out year);

                    if (year < 1900 || year > DateTime.Now.Year + 1)
                    {
                        year = 0;
                    }

                    Logger.Trace("Series Parsed. {0}", seriesName);
                    return seriesName;
                }
            }

            return String.Empty;
        }

        /// <summary>
        /// Parses proper status out of a report title
        /// </summary>
        /// <param name="title">Title of the report</param>
        /// <returns></returns>
        internal static bool ParseProper(string title)
        {
            return title.ToLower().Contains("proper");
        }

        internal static QualityTypes ParseQuality(string name)
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
                return QualityTypes.Bluray;
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
        /// Normalizes the title. removing all non-word characters as well as common tokens
        /// such as 'the' and 'and'
        /// </summary>
        /// <param name="title">title</param>
        /// <returns></returns>
        internal static string NormalizeTitle(string title)
        {
            return NormalizeRegex.Replace(title, String.Empty).ToLower();
        }

        //Note: changing case on path is a problem for running on mono/*nix
        //Not going to change the casing any more... Looks Ugly in UI anyways :P
        public static string NormalizePath(string path)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentException("Path can not be null or empty");

            var info = new FileInfo(path);

            if (info.FullName.StartsWith(@"\\")) //UNC
            {
                return info.FullName.TrimEnd('/', '\\', ' ');
            }

            return info.FullName.Trim('/', '\\', ' ');
        }

        public static NzbInfoModel ParseNzbInfo(FeedInfoModel feed, RssItem item)
        {
            NzbSiteModel site = NzbSiteModel.Parse(feed.Url.ToLower());
            return new NzbInfoModel
            {
                Id = site.ParseId(item.Link.ToString()),
                Title = item.Title,
                Site = site,
                Link = item.Link,
                Description = item.Description
            };
        }
    }
}
