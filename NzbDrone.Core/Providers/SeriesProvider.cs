using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using NzbDrone.Core.Repository;
using SubSonic.Repository;
using TvdbLib.Data;

namespace NzbDrone.Core.Providers
{
    public class SeriesProvider : ISeriesProvider
    {
        //TODO: Remove parsing of rest of tv show info we just need the show name
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
        private readonly ILog _logger;
        private readonly IRepository _sonioRepo;
        private readonly ITvDbProvider _tvDb;

        public SeriesProvider(ILog logger, IDiskProvider diskProvider, IConfigProvider configProvider, IRepository dataRepository, ITvDbProvider tvDbProvider)
        {
            _logger = logger;
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
            return _sonioRepo.Single<Series>(s => s.TvdbId == tvdbId.ToString());
        }

        public void SyncSeriesWithDisk()
        {
            foreach (string seriesFolder in _diskProvider.GetDirectories(_config.SeriesRoot))
            {
                var cleanPath = DiskProvider.CleanPath(new DirectoryInfo(seriesFolder).FullName);
                if (!_sonioRepo.Exists<Series>(s => s.Path == cleanPath))
                {
                    _logger.InfoFormat("Folder '{0} isn't mapped to a series in the database. Trying to map it.'", cleanPath);
                    AddShow(cleanPath);
                }
            }
        }

        #endregion

        private void AddShow(string path)
        {
            var searchResults = _tvDb.SearchSeries(new DirectoryInfo(path).Name);
            if (searchResults.Count != 0 && !_sonioRepo.Exists<Series>(s => s.TvdbId == searchResults[0].Id.ToString()))
                AddShow(path, _tvDb.GetSeries(searchResults[0].Id, searchResults[0].Language));
        }

        private void AddShow(string path, TvdbSeries series)
        {
            var repoSeries = new Series();
            repoSeries.TvdbId = series.Id.ToString();
            repoSeries.SeriesName = series.SeriesName;
            repoSeries.AirTimes = series.AirsTime;
            repoSeries.AirsDayOfWeek = series.AirsDayOfWeek;
            repoSeries.Overview = series.Overview;
            repoSeries.Status = series.Status;
            repoSeries.Language = series.Language != null ? series.Language.Abbriviation : string.Empty;
            repoSeries.Path = path;
            _sonioRepo.Add(repoSeries);
        }

        /// <summary>
        /// Parses a post title
        /// </summary>
        /// <param name="postTitle">Title of the report</param>
        /// <returns>TVDB id of the series this report belongs to</returns>
        public long Parse(string postTitle)
        {
            var match = ParseRegex.Match(postTitle);

            if (!match.Success)
                throw new ArgumentException(String.Format("Title doesn't match any know patterns. [{0}]", postTitle));

            //TODO: title should be mapped to a proper Series object. with tvdbId and everything even if it is not in the db or being tracked.

            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines if a series is being actively watched.
        /// </summary>
        /// <param name="id">The TVDB ID of the series</param>
        /// <returns>Whether or not the show is monitored</returns>
        public bool IsMonitored(long id)
        {
            //should just check the db for now, if it exists its being monitored.
            throw new NotImplementedException();
        }
    }
}