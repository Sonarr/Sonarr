using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.Messaging.Events;
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
    }

    public class SeriesService : ISeriesService
    {
        private readonly ISeriesRepository _seriesRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly ISceneMappingService _sceneMappingService;
        private readonly IEpisodeService _episodeService;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly Logger _logger;

        public SeriesService(ISeriesRepository seriesRepository,
                             IEventAggregator eventAggregator,
                             ISceneMappingService sceneMappingService,
                             IEpisodeService episodeService,
                             IBuildFileNames fileNameBuilder,
                             Logger logger)
        {
            _seriesRepository = seriesRepository;
            _eventAggregator = eventAggregator;
            _sceneMappingService = sceneMappingService;
            _episodeService = episodeService;
            _fileNameBuilder = fileNameBuilder;
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
                var folderName = _fileNameBuilder.GetSeriesFolder(newSeries.Title);
                newSeries.Path = Path.Combine(newSeries.RootFolderPath, folderName);
            }

            _logger.Info("Adding Series {0} Path: [{1}]", newSeries, newSeries.Path);

            newSeries.Monitored = true;
            newSeries.CleanTitle = Parser.Parser.CleanSeriesTitle(newSeries.Title);

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
    }
}
