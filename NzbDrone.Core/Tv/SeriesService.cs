using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Eventing;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.ReferenceData;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Tv
{
    public interface ISeriesService
    {
        bool IsMonitored(int id);
        Series UpdateSeriesInfo(int seriesId);
        Series FindSeries(string title);
        void AddSeries(Series newSeries);
        void UpdateFromSeriesEditor(IList<Series> editedSeries);
        Series FindByTvdbId(int tvdbId);
        void SetSeriesType(int seriesId, SeriesTypes seriesTypes);
        void DeleteSeries(int seriesId, bool deleteFiles);
    }

    public class SeriesService : ISeriesService, IHandleAsync<SeriesAddedEvent>
    {
        private readonly ISeriesRepository _seriesRepository;
        private readonly IConfigService _configService;
        private readonly IProvideSeriesInfo _seriesInfoProxy;
        private readonly IEventAggregator _eventAggregator;
        private readonly IQualityProfileService _qualityProfileService;
        private readonly Logger _logger;


        private readonly ISceneMappingService _sceneNameMappingService;

        public SeriesService(ISeriesRepository seriesRepository, IConfigService configServiceService,
                             IProvideSeriesInfo seriesInfoProxy, ISceneMappingService sceneNameMappingService,
                             IEventAggregator eventAggregator, IQualityProfileService qualityProfileService, Logger logger)
        {
            _seriesRepository = seriesRepository;
            _configService = configServiceService;
            _seriesInfoProxy = seriesInfoProxy;
            _sceneNameMappingService = sceneNameMappingService;
            _eventAggregator = eventAggregator;
            _qualityProfileService = qualityProfileService;
            _logger = logger;
        }


        public bool IsMonitored(int id)
        {
            return _seriesRepository.Get(id).Monitored;
        }


        public Series UpdateSeriesInfo(int seriesId)
        {
            var series = _seriesRepository.Get(seriesId);
            var tvDbSeries = _seriesInfoProxy.GetSeriesInfo(series.TvDbId);

            series.Title = tvDbSeries.Title;
            series.AirTime = tvDbSeries.AirTime;
            series.Overview = tvDbSeries.Overview;
            series.Status = tvDbSeries.Status;
            series.CleanTitle = Parser.NormalizeTitle(tvDbSeries.Title);
            series.LastInfoSync = DateTime.Now;
            series.Runtime = tvDbSeries.Runtime;
            series.Images = tvDbSeries.Images;
            series.Network = tvDbSeries.Network;
            series.FirstAired = tvDbSeries.FirstAired;
            _seriesRepository.Update(series);

            _eventAggregator.Publish(new SeriesUpdatedEvent(series));

            return series;
        }

        public Series FindSeries(string title)
        {
            var normalizeTitle = Parser.NormalizeTitle(title);

            var mapping = _sceneNameMappingService.GetTvDbId(normalizeTitle);
            if (mapping.HasValue)
            {
                var sceneSeries = _seriesRepository.Get(mapping.Value);
                return sceneSeries;
            }

            return _seriesRepository.GetByTitle(normalizeTitle);
        }

        public void AddSeries(Series newSeries)
        {
            Ensure.That(() => newSeries).IsNotNull();

            _logger.Info("Adding Series [{0}] Path: [{1}]", newSeries.Title, newSeries.Path);

            newSeries.Monitored = true;
            newSeries.CleanTitle = Parser.NormalizeTitle(newSeries.Title);
            if (newSeries.QualityProfileId == 0)
                newSeries.QualityProfileId = _configService.DefaultQualityProfile;

            newSeries.SeasonFolder = _configService.UseSeasonFolder;
            newSeries.BacklogSetting = BacklogSettingType.Inherit;

            _seriesRepository.Insert(newSeries);
            _eventAggregator.Publish(new SeriesAddedEvent(newSeries));
        }

        public void UpdateFromSeriesEditor(IList<Series> editedSeries)
        {
            var allSeries = _seriesRepository.All();

            foreach (var series in allSeries)
            {
                //Only update parameters that can be changed in MassEdit
                var edited = editedSeries.Single(s => ((ModelBase)s).Id == series.Id);
                series.QualityProfileId = edited.QualityProfileId;
                series.Monitored = edited.Monitored;
                series.SeasonFolder = edited.SeasonFolder;
                series.BacklogSetting = edited.BacklogSetting;
                series.Path = edited.Path;
                series.CustomStartDate = edited.CustomStartDate;

                _seriesRepository.Update(series);
            }

        }

        public void SetTvRageId(int seriesId, int tvRageId)
        {
            _seriesRepository.SetTvRageId(seriesId, tvRageId);
        }

        public Series FindByTvdbId(int tvdbId)
        {
            return _seriesRepository.FindByTvdbId(tvdbId);
        }

        public void SetSeriesType(int seriesId, SeriesTypes seriesTypes)
        {
            _seriesRepository.SetSeriesType(seriesId, seriesTypes);
        }

        public void DeleteSeries(int seriesId, bool deleteFiles)
        {
            var series = _seriesRepository.Get(seriesId);
            _seriesRepository.Delete(seriesId);
            _eventAggregator.Publish(new SeriesDeletedEvent(series, deleteFiles));
        }

        public void HandleAsync(SeriesAddedEvent message)
        {
            UpdateSeriesInfo(message.Series.Id);
        }
    }
}
