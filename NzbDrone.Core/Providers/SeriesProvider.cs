using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using PetaPoco;

namespace NzbDrone.Core.Providers
{
    public class SeriesProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ConfigProvider _configProvider;
        private readonly TvDbProvider _tvDbProvider;
        private readonly IDatabase _database;
        private readonly SceneMappingProvider _sceneNameMappingProvider;
        private readonly BannerProvider _bannerProvider;
        private static readonly Regex TimeRegex = new Regex(@"^(?<time>\d+:?\d*)\W*(?<meridiem>am|pm)?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public SeriesProvider(IDatabase database, ConfigProvider configProviderProvider,
                                TvDbProvider tvDbProviderProvider, SceneMappingProvider sceneNameMappingProvider,
                                BannerProvider bannerProvider)
        {
            _database = database;
            _configProvider = configProviderProvider;
            _tvDbProvider = tvDbProviderProvider;
            _sceneNameMappingProvider = sceneNameMappingProvider;
            _bannerProvider = bannerProvider;
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

        public virtual IList<Series> GetAllSeriesWithEpisodeCount()
        {
            var series = _database
          .Fetch<Series, QualityProfile>(@"SELECT Series.SeriesId, Series.Title, Series.CleanTitle, Series.Status, Series.Overview, Series.AirsDayOfWeek, Series.AirTimes,
                                            Series.Language, Series.Path, Series.Monitored, Series.QualityProfileId, Series.SeasonFolder, Series.BacklogSetting, Series.Network,
                                            SUM(CASE WHEN Ignored = 0 AND Airdate <= @0 THEN 1 ELSE 0 END) AS EpisodeCount,
                                            SUM(CASE WHEN Episodes.Ignored = 0 AND Episodes.EpisodeFileId > 0 AND Episodes.AirDate <= @0 THEN 1 ELSE 0 END) as EpisodeFileCount,
                                            MAX(Episodes.SeasonNumber) as SeasonCount, MIN(CASE WHEN AirDate < @0 OR Ignored = 1 THEN NULL ELSE AirDate END) as NextAiring,
                                            QualityProfiles.QualityProfileId, QualityProfiles.Name, QualityProfiles.Cutoff, QualityProfiles.SonicAllowed
                                            FROM Series
                                            INNER JOIN QualityProfiles ON Series.QualityProfileId = QualityProfiles.QualityProfileId
                                            LEFT JOIN Episodes ON Series.SeriesId = Episodes.SeriesId
                                            WHERE Series.LastInfoSync IS NOT NULL
                                            GROUP BY Series.SeriesId, Series.Title, Series.CleanTitle, Series.Status, Series.Overview, Series.AirsDayOfWeek, Series.AirTimes,
                                            Series.Language, Series.Path, Series.Monitored, Series.QualityProfileId, Series.SeasonFolder, Series.BacklogSetting, Series.Network,
                                            QualityProfiles.QualityProfileId, QualityProfiles.Name, QualityProfiles.Cutoff, QualityProfiles.SonicAllowed", DateTime.Today);

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
            var tvDbSeries = _tvDbProvider.GetSeries(seriesId, false);
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
            series.Runtime = (int)tvDbSeries.Runtime;
            series.BannerUrl = tvDbSeries.BannerPath;
            series.Network = tvDbSeries.Network;

            UpdateSeries(series);
            return series;
        }

        public virtual void AddSeries(string title, string path, int tvDbSeriesId, int qualityProfileId)
        {
            Logger.Info("Adding Series [{0}] Path: [{1}]", tvDbSeriesId, path);

            if (tvDbSeriesId <=0)
            {
                throw new ArgumentOutOfRangeException("tvDbSeriesId", tvDbSeriesId.ToString());
            }

            var repoSeries = new Series();
            repoSeries.SeriesId = tvDbSeriesId;
            repoSeries.Path = path;
            repoSeries.Monitored = true; //New shows should be monitored
            repoSeries.QualityProfileId = qualityProfileId;
            repoSeries.Title = title;
            if (qualityProfileId == 0)
                repoSeries.QualityProfileId = _configProvider.DefaultQualityProfile;

            repoSeries.SeasonFolder = _configProvider.UseSeasonFolder;
            repoSeries.BacklogSetting = BacklogSettingType.Inherit;

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
                            WHERE CleanTitle = @0", normalizeTitle).SingleOrDefault();

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

                Logger.Debug("Deleting Seasons from DB for Series: {0}", series.Title);
                _database.Delete<Season>("WHERE SeriesId=@0", seriesId);

                Logger.Debug("Deleting Episodes from DB for Series: {0}", series.Title);
                _database.Delete<Episode>("WHERE SeriesId=@0", seriesId);

                Logger.Debug("Deleting Series from DB {0}", series.Title);
                _database.Delete<Series>("WHERE SeriesId=@0", seriesId);

                Logger.Info("Successfully deleted Series [{0}]", series.Title);

                tran.Complete();
            }

            Logger.Trace("Beginning deletion of banner for SeriesID: ", seriesId);
            _bannerProvider.Delete(seriesId);
        }

        public virtual bool SeriesPathExists(string path)
        {
            return GetAllSeries().Any(s => DiskProvider.PathEquals(s.Path, path));
        }

        public virtual List<Series> SearchForSeries(string title)
        {
            var query = String.Format("%{0}%", title);

            var series = _database.Fetch<Series, QualityProfile>(@"SELECT * FROM Series
                                INNER JOIN QualityProfiles ON Series.QualityProfileId = QualityProfiles.QualityProfileId
                                WHERE Title LIKE @0", query);

            return series;
        }

        public virtual void UpdateFromSeriesEditor(IList<Series> editedSeries)
        {
            var allSeries = GetAllSeries();

            foreach(var series in allSeries)
            {
                //Only update parameters that can be changed in MassEdit
                var edited = editedSeries.Single(s => s.SeriesId == series.SeriesId);
                series.QualityProfileId = edited.QualityProfileId;
                series.Monitored = edited.Monitored;
                series.SeasonFolder = edited.SeasonFolder;
                series.BacklogSetting = edited.BacklogSetting;
                series.Path = edited.Path;
            }

            _database.UpdateMany(allSeries);
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

            DateTime dateTime;

            if (String.IsNullOrEmpty(time) || !DateTime.TryParse(time + " " + meridiem.ToUpper(), out dateTime))
                return String.Empty;

            return dateTime.ToString("hh:mm tt");
        }
    }
}