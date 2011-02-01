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
                                                         new Regex(@"(?<title>.+?)?\W(S)?(?<season>\d+)[EeXx](?<episode>\d+)\W(?!\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled)                                                        
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

                    foreach (Match matchGroup in match)
                    {
                        var parsedEpisode = new EpisodeParseResult
                        {
                            SeriesTitle = seriesName,
                            SeasonNumber = Convert.ToInt32(matchGroup.Groups["season"].Value),
                            EpisodeNumber = Convert.ToInt32(matchGroup.Groups["episode"].Value)
                        };

                        result.Add(parsedEpisode);

                        Logger.Trace("Episode Parsed. {0}", parsedEpisode);
                    }
                }
            }

            return result;
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
            if (name.Contains("xvid") || name.Contains("divx"))
            {
                if (name.Contains("bluray") || name.Contains("bdrip"))
                {
                    return QualityTypes.DVD;
                }
                return QualityTypes.TV;
            }

            if (name.Contains("bluray") || name.Contains("bdrip"))
            {
                if (name.Contains("1080p"))
                    return QualityTypes.Bluray1080p;

                return QualityTypes.Bluray720p;
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
        public static string NormalizePath(string path)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentException("Path can not be null or empty");

          var info = new FileInfo(path);

          if( info.FullName.StartsWith(@"\\")) //UNC
          {
            return info.FullName.ToLower().TrimEnd('/', '\\', ' ');
          }
          
          return info.FullName.ToLower().Trim('/', '\\', ' ');
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
