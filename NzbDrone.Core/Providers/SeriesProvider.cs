using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using SubSonic.Repository;
using TvdbLib.Data;

namespace NzbDrone.Core.Providers
{
    public class SeriesProvider
    {
        //TODO: Remove parsing of rest of tv show info we just need the show name
        //Trims all white spaces and separators from the end of the title.

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IRepository _repository;
        private readonly ConfigProvider _configProvider;
        private readonly TvDbProvider _tvDbProvider;

        public SeriesProvider(ConfigProvider configProviderProvider, IRepository repository, TvDbProvider tvDbProviderProvider)
        {
            _configProvider = configProviderProvider;
            _repository = repository;
            _tvDbProvider = tvDbProviderProvider;
        }

        public SeriesProvider()
        {
        }

        public virtual IQueryable<Series> GetAllSeries()
        {
            return _repository.All<Series>();
        }

        public virtual Series GetSeries(int seriesId)
        {
            return _repository.Single<Series>(s => s.SeriesId == seriesId);
        }

        /// <summary>
        ///   Determines if a series is being actively watched.
        /// </summary>
        /// <param name = "id">The TVDB ID of the series</param>
        /// <returns>Whether or not the show is monitored</returns>
        public virtual bool IsMonitored(long id)
        {
            return _repository.Exists<Series>(c => c.SeriesId == id && c.Monitored);
        }

        public virtual bool QualityWanted(int seriesId, QualityTypes quality)
        {
            var series = _repository.Single<Series>(seriesId);
            Logger.Trace("Series {0} is using quality profile {1}", seriesId, series.QualityProfile.Name);
            return series.QualityProfile.Allowed.Contains(quality);
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
                repoSeries.QualityProfileId = Convert.ToInt32(_configProvider.GetValue("DefaultQualityProfile", "1", true));

            repoSeries.SeasonFolder = _configProvider.UseSeasonFolder;

            _repository.Add(repoSeries);
        }

        public virtual Series FindSeries(string title)
        {
            //TODO:Add series alias support here. if a series is not found in the repo should be tried using its aliases
            var normalizeTitle = Parser.NormalizeTitle(title);
            return _repository.Single<Series>(s => s.CleanTitle == normalizeTitle);
        }

        public virtual void UpdateSeries(Series series)
        {
            _repository.Update(series);
        }

        public virtual void DeleteSeries(int seriesId)
        {
            Logger.Warn("Deleting Series [{0}]", seriesId);
            var series = _repository.Single<Series>(seriesId);

            //Delete Files, Episdes, Seasons then the Series
            //Can't use providers because episode provider needs series provider - Cyclic Dependency Injection, this will work

            //Delete History Items for any episodes that belong to this series
            Logger.Debug("Deleting History Items from DB for Series: {0}", series.SeriesId);
            var episodes = series.Episodes.Select(e => e.EpisodeId).ToList();
            episodes.ForEach(e => _repository.DeleteMany<History>(h => h.EpisodeId == e));

            //Delete all episode files from the DB for episodes in this series
            Logger.Debug("Deleting EpisodeFiles from DB for Series: {0}", series.SeriesId);
            _repository.DeleteMany(series.EpisodeFiles);

            //Delete all episodes for this series from the DB
            Logger.Debug("Deleting Episodes from DB for Series: {0}", series.SeriesId);
            _repository.DeleteMany(series.Episodes);

            //Delete seasons for this series from the DB
            Logger.Debug("Deleting Seasons from DB for Series: {0}", series.SeriesId);
            _repository.DeleteMany(series.Seasons);

            //Delete the Series
            Logger.Debug("Deleting Series from DB {0}", series.Title);
            _repository.Delete<Series>(seriesId);

            Logger.Info("Successfully deleted Series [{0}]", seriesId);
        }

        public virtual bool SeriesPathExists(string cleanPath)
        {
            if (_repository.Exists<Series>(s => s.Path == cleanPath))
                return true;

            return false;
        }

        /// <summary>
        ///   Cleans up the AirsTime Component from TheTVDB since it can be garbage that comes in.
        /// </summary>
        /// <param name = "input">The TVDB AirsTime</param>
        /// <returns>String that contains the AirTimes</returns>
        private string CleanAirsTime(string inputTime)
        {
            Regex timeRegex = new Regex(@"^(?<time>\d+:?\d*)\W*(?<meridiem>am|pm)?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var match = timeRegex.Match(inputTime);
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