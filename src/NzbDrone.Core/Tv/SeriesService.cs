using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Tv
{
    public interface ISeriesService
    {
        Series GetSeries(int seriesId);
        Series AddSeries(Series newSeries);
        Series FindByTvdbId(int tvdbId);
        Series FindByTvRageId(int tvRageId);
        Series FindByTitle(string title);
        Series FindByTitle(string title, int year);
        void SetSeriesType(int seriesId, SeriesTypes seriesTypes);
        void DeleteSeries(int seriesId, bool deleteFiles);
        List<Series> GetAllSeries();
        Series UpdateSeries(Series series);
        List<Series> UpdateSeries(List<Series> series);
        bool SeriesPathExists(string folder);
        Series SearchForAndAddNewSeries(string searchTitle);
    }

    public class SeriesService : ISeriesService
    {
        private readonly ISeriesRepository _seriesRepository;
        private readonly IConfigService _configService;
        private readonly IEventAggregator _eventAggregator;
        private readonly ISceneMappingService _sceneMappingService;
        private readonly IEpisodeService _episodeService;
        private readonly ISearchForNewSeries _searchForNewSeries;
        private readonly IProvideSeriesInfo _provideSeriesInfo;
        private readonly Logger _logger;

        public SeriesService(ISeriesRepository seriesRepository,
                             IConfigService configServiceService,
                             IEventAggregator eventAggregator,
                             ISceneMappingService sceneMappingService,
                             IEpisodeService episodeService,
                             ISearchForNewSeries searchForNewSeries,
                             IProvideSeriesInfo provideSeriesInfo,
                             Logger logger)
        {
            _seriesRepository = seriesRepository;
            _configService = configServiceService;
            _eventAggregator = eventAggregator;
            _sceneMappingService = sceneMappingService;
            _episodeService = episodeService;
            _searchForNewSeries = searchForNewSeries;
            _provideSeriesInfo = provideSeriesInfo;
            _logger = logger;
        }

        public Series GetSeries(int seriesId)
        {
            return _seriesRepository.Get(seriesId);
        }

        public Series AddSeries(Series newSeries)
        {
            Ensure.That(newSeries, () => newSeries).IsNotNull();

            if (String.IsNullOrWhiteSpace(newSeries.Path))
            {
                var folderName = FileNameBuilder.CleanFilename(newSeries.Title);
                newSeries.Path = Path.Combine(newSeries.RootFolderPath, folderName);
            }

            _logger.Info("Adding Series {0} Path: [{1}]", newSeries, newSeries.Path);

            newSeries.Monitored = true;
            if (string.IsNullOrEmpty(newSeries.CleanTitle))
            {
                newSeries.CleanTitle = Parser.Parser.CleanSeriesTitle(newSeries.Title);
            }

            _seriesRepository.Insert(newSeries);
            _eventAggregator.PublishEvent(new SeriesAddedEvent(newSeries));

            return newSeries;
        }

        public Series FindByTvdbId(int tvdbId)
        {
            return _seriesRepository.FindByTvdbId(tvdbId);
        }

        public Series FindByTvRageId(int tvRageId)
        {
            return _seriesRepository.FindByTvRageId(tvRageId);
        }

        public Series FindByTitle(string title)
        {
            var tvdbId = _sceneMappingService.GetTvDbId(title);

            if (tvdbId.HasValue)
            {
                return FindByTvdbId(tvdbId.Value);
            }

            return _seriesRepository.FindByTitle(Parser.Parser.CleanSeriesTitle(title));
        }

        public Series FindByTitle(string title, int year)
        {
            return _seriesRepository.FindByTitle(title, year);
        }

        public void SetSeriesType(int seriesId, SeriesTypes seriesTypes)
        {
            _seriesRepository.SetSeriesType(seriesId, seriesTypes);
        }

        public void DeleteSeries(int seriesId, bool deleteFiles)
        {
            var series = _seriesRepository.Get(seriesId);
            _seriesRepository.Delete(seriesId);
            _eventAggregator.PublishEvent(new SeriesDeletedEvent(series, deleteFiles));
        }

        public List<Series> GetAllSeries()
        {
            return _seriesRepository.All().ToList();
        }

        public Series UpdateSeries(Series series)
        {
            var storedSeries = GetSeries(series.Id);

            foreach (var season in series.Seasons)
            {
                var storedSeason = storedSeries.Seasons.SingleOrDefault(s => s.SeasonNumber == season.SeasonNumber);

                if (storedSeason != null && season.Monitored != storedSeason.Monitored)
                {
                    _episodeService.SetEpisodeMonitoredBySeason(series.Id, season.SeasonNumber, season.Monitored);
                }
            }

            return _seriesRepository.Update(series);
        }

        public List<Series> UpdateSeries(List<Series> series)
        {
            foreach (var s in series)
            {
                if (!String.IsNullOrWhiteSpace(s.RootFolderPath))
                {
                    var folderName = new DirectoryInfo(s.Path).Name;
                    s.Path = Path.Combine(s.RootFolderPath, folderName);
                }
            }

            _seriesRepository.UpdateMany(series);

            return series;
        }

        public bool SeriesPathExists(string folder)
        {
            return _seriesRepository.SeriesPathExists(folder);
        }

        public Series SearchForAndAddNewSeries(string searchTitle)
        {
            if (!_configService.SearchForNewSeriesOnImport)
            {
                return null;
            }

            var info = Parser.Parser.ParseTitle(searchTitle);
            if (info == null)
            {
                return null;
            }


            var seriesList = _searchForNewSeries.SearchForNewSeries(info.OriginalSeriesTitle);
            if (!seriesList.Any())
            {
                // none found
                return null;
            }

            // search for existing series by tvdbid or tvrageid
            foreach (var found in seriesList)
            {
                if (found.TvdbId != 0)
                {
                    var series = FindByTvdbId(found.TvdbId);
                    if (series != null)
                    {
                        _logger.Info("Found matching series {0}{1} for {2}", found, found.Title, searchTitle);
                        return series;
                    }
                }

                if (found.TvRageId != 0)
                {
                    var series = FindByTvRageId(found.TvRageId);
                    if (series != null)
                    {
                        _logger.Info("Found matching series {0}{1} for {2}", found, found.Title, searchTitle);
                        return series;
                    }
                }
            }
            
            // could not find existing series...

            if (_configService.AutoAddNewSeriesOnImport)
            {
                // automatically add this as a new series

                // determine clean title from source string
                string cleanTitle = info.SeriesTitle;

                // find the first series with an exact clean title
                var found = seriesList.FirstOrDefault(s=> s.CleanTitle == cleanTitle);
                if (found == null)
                {
                    // find the first series that starts with the same clean title
                    found = seriesList.FirstOrDefault(s => cleanTitle.StartsWith(s.CleanTitle));
                    if (found == null)
                    {
                        // find the first series that contains the same clean title
                        found = seriesList.FirstOrDefault(s => s.CleanTitle.Contains(cleanTitle) || cleanTitle.Contains(s.CleanTitle));
                    }
                }

                if (found != null)
                {
                    // this is a hack, use another series for the defaults for this newly added series
                    // TODO: determine how to appropriately set these defaults
                    var defaultSeries = _seriesRepository.All().FirstOrDefault();
                    if (defaultSeries == null)
                    {
                        return null;
                    }

                    // use clean title that we found from parsing
                    found.CleanTitle = cleanTitle;
                    // copy this info from our default series TODO: fix this
                    found.RootFolderPath = System.IO.Path.GetDirectoryName(defaultSeries.Path);
                    // copy this info from our default series TODO: fix this
                    found.QualityProfileId = defaultSeries.QualityProfileId;

                    // add new series and return it
                    var newSeries = AddSeries(found);
                    // return new series
                    return newSeries;
                }
            }

            // series not found
            return null;
        }

    }
}
