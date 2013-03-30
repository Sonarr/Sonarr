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
        void AddSeries(string title, string path, int tvDbSeriesId, int qualityProfileId, DateTime? airedAfter);
        void UpdateFromSeriesEditor(IList<Series> editedSeries);
        Series FindByTvdbId(int tvdbId);
        void SetSeriesType(int seriesId, SeriesTypes seriesTypes);
        void DeleteSeries(int seriesId, bool deleteFiles);
    }

    public class SeriesService : ISeriesService, IHandleAsync<SeriesAddedEvent>
    {
        private readonly ISeriesRepository _seriesRepository;
        private readonly IConfigService _configService;
        private readonly TvDbProxy _tvDbProxy;
        private readonly TvRageMappingProvider _tvRageMappingProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly IQualityProfileService _qualityProfileService;
        private readonly Logger _logger;


        private readonly ISceneMappingService _sceneNameMappingService;

        public SeriesService(ISeriesRepository seriesRepository, IConfigService configServiceService,
                                TvDbProxy tvDbProxyProxy, ISceneMappingService sceneNameMappingService,
                                TvRageMappingProvider tvRageMappingProvider, IEventAggregator eventAggregator, IQualityProfileService qualityProfileService, Logger logger)
        {
            _seriesRepository = seriesRepository;
            _configService = configServiceService;
            _tvDbProxy = tvDbProxyProxy;
            _sceneNameMappingService = sceneNameMappingService;
            _tvRageMappingProvider = tvRageMappingProvider;
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
            var tvDbSeries = _tvDbProxy.GetSeries(series.TvDbId);

            series.Title = tvDbSeries.Title;
            series.AirTime = tvDbSeries.AirTime;
            series.Overview = tvDbSeries.Overview;
            series.Status = tvDbSeries.Status;
            series.Language = tvDbSeries.Language;
            series.CleanTitle = tvDbSeries.CleanTitle;
            series.LastInfoSync = DateTime.Now;
            series.Runtime = tvDbSeries.Runtime;
            series.Covers = tvDbSeries.Covers;
            series.Network = tvDbSeries.Network;
            series.FirstAired = tvDbSeries.FirstAired;

            try
            {
                if (series.TvRageId == 0)
                {
                    series = _tvRageMappingProvider.FindMatchingTvRageSeries(series);
                }
            }

            catch (Exception ex)
            {
                _logger.ErrorException("Error getting TvRage information for series: " + series.Title, ex);
            }

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

        public void AddSeries(string title, string path, int tvDbSeriesId, int qualityProfileId, DateTime? airedAfter)
        {
            _logger.Info("Adding Series [{0}] Path: [{1}]", tvDbSeriesId, path);

            Ensure.That(() => tvDbSeriesId).IsGreaterThan(0);
            Ensure.That(() => title).IsNotNullOrWhiteSpace();
            Ensure.That(() => path).IsNotNullOrWhiteSpace();

            var repoSeries = new Series();
            repoSeries.TvDbId = tvDbSeriesId;
            repoSeries.Path = path;
            repoSeries.Monitored = true;
            repoSeries.QualityProfileId = qualityProfileId;
            repoSeries.Title = title;
            repoSeries.CleanTitle = Parser.NormalizeTitle(title);
            if (qualityProfileId == 0)
                repoSeries.QualityProfileId = _configService.DefaultQualityProfile;

            repoSeries.QualityProfile = _qualityProfileService.Get(repoSeries.QualityProfileId);
            repoSeries.SeasonFolder = _configService.UseSeasonFolder;
            repoSeries.BacklogSetting = BacklogSettingType.Inherit;

            if (airedAfter.HasValue)
                repoSeries.CustomStartDate = airedAfter;

            //Todo: Allow the user to set this as part of the addition process.
            repoSeries.Language = "en";

            _seriesRepository.Insert(repoSeries);

            _eventAggregator.Publish(new SeriesAddedEvent(repoSeries));
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
