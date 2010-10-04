using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using NLog;
using NzbDrone.Core.Repository;
using SubSonic.Repository;
using TvdbLib.Data;

namespace NzbDrone.Core.Providers
{
    public class SeriesProvider : ISeriesProvider
    {
        //TODO: Remove parsing of rest of tv show info we just need the show name

        //Trims all white spaces and separators from the end of the title.
        private static readonly Regex CleanTitleRegex = new Regex(@"[\s.][^a-z]*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ParseRegex = new Regex(@"(?<showName>.*)
(?:
  s(?<seasonNumber>\d+)e(?<episodeNumber>\d+)-?e(?<episodeNumber2>\d+)
| s(?<seasonNumber>\d+)e(?<episodeNumber>\d+)
|  (?<seasonNumber>\d+)x(?<episodeNumber>\d+)
|  (?<airDate>\d{4}.\d{2}.\d{2})
)
(?:
  (?<episodeName>.*?)
  (?<release>
     (?:hdtv|pdtv|xvid|ws|720p|x264|bdrip|dvdrip|dsr|proper)
     .*)
| (?<episodeName>.*)
)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);



        private readonly IConfigProvider _config;
        private readonly IDiskProvider _diskProvider;
        private readonly IRepository _sonioRepo;
        private readonly ITvDbProvider _tvDb;
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly Regex CleanUpRegex = new Regex(@"((\s|^)the(\s|$))|((\s|^)and(\s|$))|[^a-z]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public SeriesProvider(IDiskProvider diskProvider, IConfigProvider configProvider, IRepository dataRepository, ITvDbProvider tvDbProvider)
        {
            _diskProvider = diskProvider;
            _config = configProvider;
            _sonioRepo = dataRepository;
            _tvDb = tvDbProvider;
        }

        #region ISeriesProvider Members

        public IQueryable<Series> GetSeries()
        {
            return _sonioRepo.All<Series>();
        }

        public Series GetSeries(long tvdbId)
        {
            return _sonioRepo.Single<Series>(s => s.TvdbId == tvdbId);
        }

        /// <summary>
        /// Determines if a series is being actively watched.
        /// </summary>
        /// <param name="id">The TVDB ID of the series</param>
        /// <returns>Whether or not the show is monitored</returns>
        public bool IsMonitored(long id)
        {
            return _sonioRepo.Exists<Series>(c => c.TvdbId == id && c.Monitored);
        }

        /// <summary>
        /// Parses series name out of a post title
        /// </summary>
        /// <param name="postTitle">Title of the report</param>
        /// <returns>Name series this report belongs to</returns>
        public static string ParseTitle(string postTitle)
        {
            var match = ParseRegex.Match(postTitle);

            if (!match.Success)
                throw new ArgumentException(String.Format("Title doesn't match any know patterns. [{0}]", postTitle));

            return CleanTitleRegex.Replace(match.Groups["showName"].Value, String.Empty).Replace(".", " ");
        }

        public void SyncSeriesWithDisk()
        {
            if (String.IsNullOrEmpty(_config.SeriesRoot))
                throw new InvalidOperationException("TV Series folder is not configured yet.");

            foreach (string seriesFolder in GetUnmappedFolders())
            {
                Logger.Info("Folder '{0}' isn't mapped to a series in the database. Trying to map it.'", seriesFolder);
                var mappedSeries = MapPathToSeries(seriesFolder);

                if (mappedSeries == null)
                {
                    Logger.Warn("Unable to find a matching series for '{0}'", seriesFolder);
                    break;
                }

                if (!_sonioRepo.Exists<Series>(s => s.TvdbId == mappedSeries.Id))
                {
                    RegisterSeries(seriesFolder, mappedSeries);
                }
                else
                {
                    Logger.Warn("Folder '{0}' mapped to '{1}' which is already another folder assigned to it.'", seriesFolder, mappedSeries.SeriesName);
                }

            }
        }

        public List<String> GetUnmappedFolders()
        {
            var results = new List<String>();
            foreach (string seriesFolder in _diskProvider.GetDirectories(_config.SeriesRoot))
            {
                var cleanPath = DiskProvider.CleanPath(new DirectoryInfo(seriesFolder).FullName);
                if (!_sonioRepo.Exists<Series>(s => s.Path == cleanPath))
                {
                    results.Add(cleanPath);
                }
            }

            return results;
        }

        public TvdbSeries MapPathToSeries(string path)
        {
            var seriesPath = new DirectoryInfo(path);
            var searchResults = _tvDb.GetSeries(seriesPath.Name);

            if (searchResults == null)
                return null;

            return _tvDb.GetSeries(searchResults.Id, searchResults.Language);
        }


        public void RegisterSeries(string path, TvdbSeries series)
        {
            Logger.Info("registering '{0}' with [{1}]-{2}", path, series.Id, series.SeriesName);
            var repoSeries = new Series();
            repoSeries.TvdbId = series.Id;
            repoSeries.Title = series.SeriesName;
            repoSeries.AirTimes = series.AirsTime;
            repoSeries.AirsDayOfWeek = series.AirsDayOfWeek;
            repoSeries.Overview = series.Overview;
            repoSeries.Status = series.Status;
            repoSeries.Language = series.Language != null ? series.Language.Abbriviation : string.Empty;
            repoSeries.Path = path;
            repoSeries.CleanTitle = CleanUpRegex.Replace(series.SeriesName, "").ToLower();
            _sonioRepo.Add(repoSeries);
        }

        #endregion

        #region Static Helpers



        #endregion
    }
}