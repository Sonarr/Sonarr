using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Eventing;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
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
    }

    public class SeriesService : ISeriesService
    {
        private readonly ISeriesRepository _seriesRepository;
        private readonly ConfigProvider _configProvider;
        private readonly TvDbProvider _tvDbProvider;
        private readonly MetadataProvider _metadataProvider;
        private readonly TvRageMappingProvider _tvRageMappingProvider;
        private readonly IEventAggregator _eventAggregator;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly SceneMappingProvider _sceneNameMappingProvider;

        public SeriesService(ISeriesRepository seriesRepository, ConfigProvider configProviderProvider,
                                TvDbProvider tvDbProviderProvider, SceneMappingProvider sceneNameMappingProvider, MetadataProvider metadataProvider,
                                TvRageMappingProvider tvRageMappingProvider, IEventAggregator eventAggregator)
        {
            _seriesRepository = seriesRepository;
            _configProvider = configProviderProvider;
            _tvDbProvider = tvDbProviderProvider;
            _sceneNameMappingProvider = sceneNameMappingProvider;
            _metadataProvider = metadataProvider;
            _tvRageMappingProvider = tvRageMappingProvider;
            _eventAggregator = eventAggregator;
        }


        public bool IsMonitored(int id)
        {
            return _seriesRepository.Get(id).Monitored;
        }


        public Series UpdateSeriesInfo(int seriesId)
        {
            var tvDbSeries = _tvDbProvider.GetSeries(seriesId, false, true);
            var series = _seriesRepository.Get(seriesId);

            series.SeriesId = tvDbSeries.Id;
            series.Title = tvDbSeries.SeriesName;
            series.AirTime = CleanAirsTime(tvDbSeries.AirsTime);
            series.AirsDayOfWeek = tvDbSeries.AirsDayOfWeek;
            series.Overview = tvDbSeries.Overview;
            series.Status = tvDbSeries.Status;
            series.Language = tvDbSeries.Language != null ? tvDbSeries.Language.Abbriviation : string.Empty;
            series.CleanTitle = Parser.NormalizeTitle(tvDbSeries.SeriesName);
            series.LastInfoSync = DateTime.Now;
            series.Runtime = (int)tvDbSeries.Runtime;
            series.BannerUrl = tvDbSeries.BannerPath;
            series.Network = tvDbSeries.Network;

            if (tvDbSeries.FirstAired.Year > 1900)
                series.FirstAired = tvDbSeries.FirstAired.Date;
            else
                series.FirstAired = null;

            try
            {
                if (series.TvRageId == 0)
                    series = _tvRageMappingProvider.FindMatchingTvRageSeries(series);
            }

            catch (Exception ex)
            {
                logger.ErrorException("Error getting TvRage information for series: " + series.Title, ex);
            }

            _seriesRepository.Update(series);
            _metadataProvider.CreateForSeries(series, tvDbSeries);

            return series;
        }

        public Series FindSeries(string title)
        {
            var normalizeTitle = Parser.NormalizeTitle(title);

            var mapping = _sceneNameMappingProvider.GetSeriesId(normalizeTitle);
            if (mapping.HasValue)
            {
                var sceneSeries = _seriesRepository.Get(mapping.Value);
                return sceneSeries;
            }

            return _seriesRepository.GetByTitle(normalizeTitle);
        }

        public void AddSeries(string title, string path, int tvDbSeriesId, int qualityProfileId, DateTime? airedAfter)
        {
            logger.Info("Adding Series [{0}] Path: [{1}]", tvDbSeriesId, path);

            Ensure.That(() => tvDbSeriesId).IsGreaterThan(0);
            Ensure.That(() => title).IsNotNullOrWhiteSpace();
            Ensure.That(() => path).IsNotNullOrWhiteSpace();

            var repoSeries = new Series();
            repoSeries.SeriesId = tvDbSeriesId;
            repoSeries.Path = path;
            repoSeries.Monitored = true;
            repoSeries.QualityProfileId = qualityProfileId;
            repoSeries.Title = title;
            if (qualityProfileId == 0)
                repoSeries.QualityProfileId = _configProvider.DefaultQualityProfile;

            repoSeries.SeasonFolder = _configProvider.UseSeasonFolder;
            repoSeries.BacklogSetting = BacklogSettingType.Inherit;

            if (airedAfter.HasValue)
                repoSeries.CustomStartDate = airedAfter;

            _seriesRepository.Insert(repoSeries);

            _eventAggregator.Publish(new SeriesAddedEvent(repoSeries));
        }


        public void UpdateFromSeriesEditor(IList<Series> editedSeries)
        {
            var allSeries = _seriesRepository.All();

            foreach (var series in allSeries)
            {
                //Only update parameters that can be changed in MassEdit
                var edited = editedSeries.Single(s => s.SeriesId == series.SeriesId);
                series.QualityProfileId = edited.QualityProfileId;
                series.Monitored = edited.Monitored;
                series.SeasonFolder = edited.SeasonFolder;
                series.BacklogSetting = edited.BacklogSetting;
                series.Path = edited.Path;
                series.CustomStartDate = edited.CustomStartDate;

                _seriesRepository.Update(series);
            }

        }

        /// <summary>
        ///   Cleans up the AirsTime Component from TheTVDB since it can be garbage that comes in.
        /// </summary>
        /// <param name = "rawTime">The TVDB AirsTime</param>
        /// <returns>String that contains the AirTimes</returns>

        private static readonly Regex timeRegex = new Regex(@"^(?<time>\d+:?\d*)\W*(?<meridiem>am|pm)?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static string CleanAirsTime(string rawTime)
        {
            var match = timeRegex.Match(rawTime);
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