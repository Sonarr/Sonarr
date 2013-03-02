using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Core.Tv;
using NzbDrone.Common;

namespace NzbDrone.Core.Providers
{
    public class TvDbProvider
    {
        public const string TVDB_APIKEY = "5D2D188E86E07F4F";
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();



        private readonly Tvdb.Tvdb _handlerV2;


        public TvDbProvider()
        {
            _handlerV2 = new Tvdb.Tvdb(TVDB_APIKEY);
        }

        public virtual List<Series> SearchSeries(string title)
        {
            logger.Debug("Searching TVDB for '{0}'", title);

            if (title.Contains(" & "))
            {
                title = title.Replace(" & ", " ");
            }

            var searchResult = _handlerV2.SearchSeries(title).Select(tvdbSeriesSearchItem => new Series
                {
                    TvDbId = tvdbSeriesSearchItem.seriesid,
                    Title = tvdbSeriesSearchItem.SeriesName,
                    FirstAired = tvdbSeriesSearchItem.FirstAired,
                    Overview = tvdbSeriesSearchItem.Overview
                }).ToList();

            logger.Debug("Search for '{0}' returned {1} possible results", title, searchResult.Count);
            return searchResult;
        }

        public virtual Series GetSeries(int tvDbSeriesId)
        {

            var tvDbSeries = _handlerV2.GetSeriesBaseRecord("http://thetvdb.com", tvDbSeriesId);

            var series = new Series();

            series.Title = tvDbSeries.SeriesName;
            series.AirTime = CleanAirsTime(tvDbSeries.Airs_Time);
            series.Overview = tvDbSeries.Overview;
            series.Status = tvDbSeries.Status;
            series.Language = tvDbSeries.Language ?? string.Empty;
            series.CleanTitle = Parser.NormalizeTitle(tvDbSeries.SeriesName);
            series.LastInfoSync = DateTime.Now;
            
            if (tvDbSeries.Runtime.HasValue)
            {
                series.Runtime = Convert.ToInt32(tvDbSeries.Runtime);
            }
            
            series.BannerUrl = tvDbSeries.banner;
            series.Network = tvDbSeries.Network;

            if (tvDbSeries.FirstAired.HasValue && tvDbSeries.FirstAired.Value.Year > 1900)
            {
                series.FirstAired = tvDbSeries.FirstAired.Value.Date;
            }
            else
            {
                series.FirstAired = null;
            }

            return series;

        }

        public virtual IList<Episode> GetEpisodes(int tvDbSeriesId)
        {


            var seriesRecord = _handlerV2.GetSeriesFullRecord("http://thetvdb.com", tvDbSeriesId);

            var tvdbEpisodes = seriesRecord.Episodes.OrderByDescending(e => e.FirstAired).ThenByDescending(e => e.EpisodeName)
                     .GroupBy(e => e.seriesid.ToString("000000") + e.SeasonNumber.ToString("000") + e.EpisodeNumber.ToString("000"))
                     .Select(e => e.First());


            var episodes = new List<Episode>();

            foreach (var tvDbEpisode in tvdbEpisodes)
            {
                var episode = new Episode();

                episode.TvDbEpisodeId = tvDbEpisode.id;
                episode.EpisodeNumber = tvDbEpisode.EpisodeNumber;
                episode.SeasonNumber = tvDbEpisode.SeasonNumber;
                episode.AbsoluteEpisodeNumber = tvDbEpisode.absolute_number.ParseInt32();
                episode.Title = tvDbEpisode.EpisodeName;
                episode.Overview = tvDbEpisode.Overview;

                if (tvDbEpisode.FirstAired.Year > 1900)
                {
                    episode.AirDate = tvDbEpisode.FirstAired.Date;
                }
                else
                {
                    episode.AirDate = null;
                }

                episodes.Add(episode);
            }

            return episodes;
        }

        /// <summary>
        ///   Cleans up the AirsTime Component from TheTVDB since it can be garbage that comes in.
        /// </summary>
        /// <param name = "rawTime">The TVDB AirsTime</param>
        /// <returns>String that contains the AirTimes</returns>

        private static readonly Regex timeRegex = new Regex(@"^(?<time>\d+:?\d*)\W*(?<meridiem>am|pm)?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static string CleanAirsTime(string rawTime)
        {
            var match = timeRegex.Match(rawTime);
            var time = match.Groups["time"].Value;
            var meridiem = match.Groups["meridiem"].Value;

            //Lets assume that a string that doesn't contain a Merideim is aired at night... So we'll add it
            if (String.IsNullOrEmpty(meridiem))
                meridiem = "PM";

            DateTime dateTime;

            if (String.IsNullOrEmpty(time) || !DateTime.TryParse(time + " " + meridiem.ToUpper(), out dateTime))
                return String.Empty;

            return dateTime.ToString("hh:mm tt");
        }
    }
}