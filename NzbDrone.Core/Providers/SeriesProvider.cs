using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
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
        private readonly QualityProvider _qualityProvider;
        private readonly SceneMappingProvider _sceneNameMappingProvider;
        private static readonly Regex TimeRegex = new Regex(@"^(?<time>\d+:?\d*)\W*(?<meridiem>am|pm)?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public SeriesProvider(IDatabase database, ConfigProvider configProviderProvider, QualityProvider qualityProvider,
                                TvDbProvider tvDbProviderProvider, SceneMappingProvider sceneNameMappingProvider)
        {
            _database = database;
            _configProvider = configProviderProvider;
            _tvDbProvider = tvDbProviderProvider;
            _qualityProvider = qualityProvider;
            _sceneNameMappingProvider = sceneNameMappingProvider;
        }

        public SeriesProvider()
        {
        }

        public virtual IList<Series> GetAllSeries()
        {
            var series = _database.Fetch<Series>();
            series.ForEach(c => c.QualityProfile = _qualityProvider.Get(c.QualityProfileId));
            return series;
        }

        public virtual Series GetSeries(int seriesId)
        {
            var series = _database.Single<Series>("WHERE seriesId= @0", seriesId);
            series.QualityProfile = _qualityProvider.Get(series.QualityProfileId);

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

        public virtual TvdbSeries MapPathToSeries(string path)
        {
            var seriesPath = new DirectoryInfo(path);
            var searchResults = _tvDbProvider.GetSeries(seriesPath.Name);

            if (searchResults == null)
                return null;

            return _tvDbProvider.GetSeries(searchResults.Id, false);
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
            var normalizeTitle = Parser.NormalizeTitle(title);

            var seriesId = _sceneNameMappingProvider.GetSeriesId(normalizeTitle);
            if (seriesId != null)
            {
                return GetSeries(seriesId.Value);
            }

            var series = _database.FirstOrDefault<Series>("WHERE CleanTitle = @0", normalizeTitle);

            if (series != null)
            {
                series.QualityProfile = _qualityProvider.Get(series.QualityProfileId);
                return series;
            }

            return null;
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