using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using SubSonic.Repository;
using TvdbLib.Data;

namespace NzbDrone.Core.Providers
{
    public class SeriesProvider : ISeriesProvider
    {
        //TODO: Remove parsing of rest of tv show info we just need the show name

        //Trims all white spaces and separators from the end of the title.

        private readonly IConfigProvider _config;
        private readonly IDiskProvider _diskProvider;
        private readonly IRepository _sonioRepo;
        private readonly ITvDbProvider _tvDb;
        private readonly IQualityProvider _quality;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SeriesProvider(IDiskProvider diskProvider, IConfigProvider configProvider,
            IRepository dataRepository, ITvDbProvider tvDbProvider, IQualityProvider quality)
        {
            _diskProvider = diskProvider;
            _config = configProvider;
            _sonioRepo = dataRepository;
            _tvDb = tvDbProvider;
            _quality = quality;
        }

        #region ISeriesProvider Members

        public IQueryable<Series> GetAllSeries()
        {
            return _sonioRepo.All<Series>();
        }

        public Series GetSeries(int seriesId)
        {
            return _sonioRepo.Single<Series>(s => s.SeriesId == seriesId);
        }

        /// <summary>
        /// Determines if a series is being actively watched.
        /// </summary>
        /// <param name="id">The TVDB ID of the series</param>
        /// <returns>Whether or not the show is monitored</returns>
        public bool IsMonitored(long id)
        {
            return _sonioRepo.Exists<Series>(c => c.SeriesId == id && c.Monitored);
        }

        public bool QualityWanted(int seriesId, QualityTypes quality)
        {
            var series = _sonioRepo.Single<Series>(seriesId);
            var profile = _quality.Find(series.QualityProfileId);

            return profile.Allowed.Contains(quality);
        }

        public Dictionary<Guid, String> GetUnmappedFolders()
        {
            Logger.Debug("Generating list of unmapped folders");
            if (String.IsNullOrEmpty(_config.SeriesRoot))
                throw new InvalidOperationException("TV Series folder is not configured yet.");

            var results = new Dictionary<Guid, String>();
            foreach (string seriesFolder in _diskProvider.GetDirectories(_config.SeriesRoot))
            {
                var cleanPath = Parser.NormalizePath(new DirectoryInfo(seriesFolder).FullName);
                if (!_sonioRepo.Exists<Series>(s => s.Path == cleanPath))
                {
                    results.Add(Guid.NewGuid(), cleanPath);
                }
            }

            Logger.Debug("{0} unmapped folders detected.", results.Count);
            return results;
        }

        public TvdbSeries MapPathToSeries(string path)
        {
            var seriesPath = new DirectoryInfo(path);
            var searchResults = _tvDb.GetSeries(seriesPath.Name);

            if (searchResults == null)
                return null;

            return _tvDb.GetSeries(searchResults.Id, false);
        }

        public void AddSeries(string path, TvdbSeries series)
        {
            Logger.Info("Adding Series [{0}]:{1} Path: {2}", series.Id, series.SeriesName, path);
            var repoSeries = new Series();
            repoSeries.SeriesId = series.Id;
            repoSeries.Title = series.SeriesName;
            repoSeries.AirTimes = series.AirsTime;
            repoSeries.AirsDayOfWeek = series.AirsDayOfWeek;
            repoSeries.Overview = series.Overview;
            repoSeries.Status = series.Status;
            repoSeries.Language = series.Language != null ? series.Language.Abbriviation : string.Empty;
            repoSeries.Path = path;
            repoSeries.CleanTitle = Parser.NormalizeTitle(series.SeriesName);
            repoSeries.Monitored = true; //New shows should be monitored
            repoSeries.QualityProfileId = Convert.ToInt32(_config.GetValue("DefaultQualityProfile", "1", true));
            _sonioRepo.Add(repoSeries);
        }

        public Series FindSeries(string cleanTitle)
        {
            return _sonioRepo.Single<Series>(s => s.CleanTitle == cleanTitle);
        }

        public void UpdateSeries(Series series)
        {
            _sonioRepo.Update(series);
        }

        public void DeleteSeries(int seriesId)
        {
            var series = _sonioRepo.Single<Series>(seriesId);

            //Delete Files, Episdes, Seasons then the Series
            //Can't use providers because episode provider needs series provider - Cyclic Dependency Injection, this will work

            Logger.Debug("Deleting EpisodeFiles from DB for Series: {0}", series.SeriesId);
            _sonioRepo.DeleteMany(series.Files);

            Logger.Debug("Deleting Episodes from DB for Series: {0}", series.SeriesId);
            _sonioRepo.DeleteMany(series.Episodes);

            Logger.Debug("Deleting Seasons from DB for Series: {0}", series.SeriesId);
            _sonioRepo.DeleteMany(series.Seasons);

            Logger.Debug("Deleting Series from DB {0}", series.Title);
            _sonioRepo.Delete<Series>(seriesId);
        }

        #endregion

        #region Static Helpers



        #endregion
    }
}