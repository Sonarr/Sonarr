using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using PetaPoco;
using TvdbLib.Data;

namespace NzbDrone.Core.Providers
{
    public class SeriesProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ConfigProvider _configProvider;
        private readonly TvDbProvider _tvDbProvider;
        private readonly IDatabase _database;
        private readonly SceneMappingProvider _sceneNameMappingProvider;
        private static readonly Regex TimeRegex = new Regex(@"^(?<time>\d+:?\d*)\W*(?<meridiem>am|pm)?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public SeriesProvider(IDatabase database, ConfigProvider configProviderProvider,
                                TvDbProvider tvDbProviderProvider, SceneMappingProvider sceneNameMappingProvider)
        {
            _database = database;
            _configProvider = configProviderProvider;
            _tvDbProvider = tvDbProviderProvider;
            _sceneNameMappingProvider = sceneNameMappingProvider;
        }

        public SeriesProvider()
        {
        }

        public virtual IList<Series> GetAllSeries()
        {
            var series = _database.Fetch<Series, QualityProfile>(@"SELECT * FROM Series 
                            INNER JOIN QualityProfiles ON Series.QualityProfileId = QualityProfiles.QualityProfileId");

            return series;
        }

        public virtual IList<Series> GetAllSeriesWithEpisodeCount(bool ignoreSpecials)
        {
            var seasonNumber = 0;

            if (!ignoreSpecials)
                seasonNumber = -1;

            var series = _database
      .Fetch<Series, QualityProfile>(@"SELECT Series.*, SUM(CASE WHEN Ignored = 0 THEN 1 ELSE 0 END) AS EpisodeCount,
SUM(CASE WHEN Ignored = 0 AND EpisodeFileId > 0 THEN 1 ELSE 0 END) as EpisodeFileCount,
COUNT (DISTINCT(CASE WHEN SeasonNumber = 0 THEN null ELSE SeasonNumber END)) as SeasonCount,
QualityProfiles.*
FROM Series
INNER JOIN QualityProfiles ON Series.QualityProfileId = QualityProfiles.QualityProfileId
JOIN Episodes ON Series.SeriesId = Episodes.SeriesId
GROUP BY seriesId");

            return series;
        }

        public virtual Series GetSeries(int seriesId)
        {
            var series = _database.Fetch<Series, QualityProfile>(@"SELECT * FROM Series
                            INNER JOIN QualityProfiles ON Series.QualityProfileId = QualityProfiles.QualityProfileId
                            WHERE seriesId= @0", seriesId).Single();

            return series;
        }

        /// <summary>
        ///   Determines if a series is being actively watched.
        /// </summary>
        /// <param name = "id">The TVDB ID of the series</param>
        /// <returns>Whether or not the show is monitored</returns>
        public virtual bool IsMonitored(long id)
        {
            return GetAllSeries().Any(c => c.SeriesId == id && c.Monitored);
        }

        public virtual Series UpdateSeriesInfo(int seriesId)
        {
            var tvDbSeries = _tvDbProvider.GetSeries(seriesId, true);
            var series = GetSeries(seriesId);

            series.SeriesId = tvDbSeries.Id;
            series.Title = tvDbSeries.SeriesName;
            series.AirTimes = CleanAirsTime(tvDbSeries.AirsTime);
            series.AirsDayOfWeek = tvDbSeries.AirsDayOfWeek;
            series.Overview = tvDbSeries.Overview;
            series.Status = tvDbSeries.Status;
            series.Language = tvDbSeries.Language != null ? tvDbSeries.Language.Abbriviation : string.Empty;
            series.CleanTitle = Parser.NormalizeTitle(tvDbSeries.SeriesName);
            series.LastInfoSync = DateTime.Now;

            UpdateSeries(series);
            return series;
        }

        public virtual void AddSeries(string path, int tvDbSeriesId, int qualityProfileId)
        {
            Logger.Info("Adding Series [{0}] Path: [{1}]", tvDbSeriesId, path);

            var repoSeries = new Series();
            repoSeries.SeriesId = tvDbSeriesId;
            repoSeries.Path = path;
            repoSeries.Monitored = true; //New shows should be monitored
            repoSeries.QualityProfileId = qualityProfileId;
            if (qualityProfileId == 0)
                repoSeries.QualityProfileId = Convert.ToInt32(_configProvider.GetValue("DefaultQualityProfile", "1"));

            repoSeries.SeasonFolder = _configProvider.UseSeasonFolder;

            _database.Insert(repoSeries);
        }

        public virtual Series FindSeries(string title)
        {
            try
            {
                var normalizeTitle = Parser.NormalizeTitle(title);

                var seriesId = _sceneNameMappingProvider.GetSeriesId(normalizeTitle);
                if (seriesId != null)
                {
                    return GetSeries(seriesId.Value);
                }

                var series = _database.Fetch<Series, QualityProfile>(@"SELECT * FROM Series
                            INNER JOIN QualityProfiles ON Series.QualityProfileId = QualityProfiles.QualityProfileId
                            WHERE CleanTitle = @0", normalizeTitle).FirstOrDefault();

                return series;
            }


            catch (InvalidOperationException)
            {
                //This will catch InvalidOperationExceptions(Sequence contains no element) 
                //that may be thrown for GetSeries due to the series being in SceneMapping, but not in the users Database
                return null;
            }
        }

        public virtual void UpdateSeries(Series series)
        {
            _database.Update(series);
        }

        public virtual void DeleteSeries(int seriesId)
        {
            var series = GetSeries(seriesId);
            Logger.Warn("Deleting Series [{0}]", series.Title);

            using (var tran = _database.GetTransaction())
            {
                //Delete History, Files, Episodes, Seasons then the Series

                Logger.Debug("Deleting History Items from DB for Series: {0}", series.Title);
                _database.Delete<History>("WHERE SeriesId=@0", seriesId);

                Logger.Debug("Deleting EpisodeFiles from DB for Series: {0}", series.Title);
                _database.Delete<EpisodeFile>("WHERE SeriesId=@0", seriesId);

                Logger.Debug("Deleting Episodes from DB for Series: {0}", series.Title);
                _database.Delete<Episode>("WHERE SeriesId=@0", seriesId);

                Logger.Debug("Deleting Series from DB {0}", series.Title);
                _database.Delete<Series>("WHERE SeriesId=@0", seriesId);

                Logger.Info("Successfully deleted Series [{0}]", series.Title);

                tran.Complete();
            }
        }

        public virtual bool SeriesPathExists(string cleanPath)
        {
            if (GetAllSeries().Any(s => s.Path == cleanPath))
                return true;

            return false;
        }

        /// <summary>
        ///   Cleans up the AirsTime Component from TheTVDB since it can be garbage that comes in.
        /// </summary>
        /// <param name = "rawTime">The TVDB AirsTime</param>
        /// <returns>String that contains the AirTimes</returns>
        private static string CleanAirsTime(string rawTime)
        {
            var match = TimeRegex.Match(rawTime);
            var time = match.Groups["time"].Value;
            var meridiem = match.Groups["meridiem"].Value;

            //Lets assume that a string that doesn't contain a Merideim is aired at night... So we'll add it
            if (String.IsNullOrEmpty(meridiem))
                meridiem = "PM";

            if (String.IsNullOrEmpty(time))
                return String.Empty;

            var dateTime = DateTime.Parse(time + " " + meridiem.ToUpper());
            return dateTime.ToString("hh:mm tt");
        }
    }
}