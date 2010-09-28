using System.IO;
using System.Linq;
using log4net;
using NzbDrone.Core.Repository;
using SubSonic.Repository;
using TvdbLib.Data;

namespace NzbDrone.Core.Providers
{
    public class SeriesProvider : ISeriesProvider
    {
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

        public Series GetSeries(int tvdbId)
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
            if (searchResults.Count != 0 && !_sonioRepo.Exists<Series>(s => s.TvdbId == searchResults[0].Id.ToString())) AddShow(path, _tvDb.GetSeries(searchResults[0].Id, searchResults[0].Language));
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
    }
}