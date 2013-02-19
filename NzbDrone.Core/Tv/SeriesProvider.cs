using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using PetaPoco;

namespace NzbDrone.Core.Tv
{
    public class SeriesProvider
    {
        private readonly ISeriesRepository _seriesRepository;
        private readonly ConfigProvider _configProvider;
        private readonly TvDbProvider _tvDbProvider;
        private readonly SceneMappingProvider _sceneNameMappingProvider;
        private readonly BannerProvider _bannerProvider;
        private readonly MetadataProvider _metadataProvider;
        private readonly TvRageMappingProvider _tvRageMappingProvider;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly Regex TimeRegex = new Regex(@"^(?<time>\d+:?\d*)\W*(?<meridiem>am|pm)?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public SeriesProvider(ISeriesRepository seriesRepository, ConfigProvider configProviderProvider,
                                TvDbProvider tvDbProviderProvider, SceneMappingProvider sceneNameMappingProvider,
                                BannerProvider bannerProvider, MetadataProvider metadataProvider,
                                TvRageMappingProvider tvRageMappingProvider)
        {
            _seriesRepository = seriesRepository;
            _configProvider = configProviderProvider;
            _tvDbProvider = tvDbProviderProvider;
            _sceneNameMappingProvider = sceneNameMappingProvider;
            _bannerProvider = bannerProvider;
            _metadataProvider = metadataProvider;
            _tvRageMappingProvider = tvRageMappingProvider;
        }


        public bool IsMonitored(int id)
        {
            return _seriesRepository.Get(id).Monitored;
        }

        public virtual Series UpdateSeriesInfo(int seriesId)
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

        public void AddSeries(string title, string path, int tvDbSeriesId, int qualityProfileId, DateTime? airedAfter)
        {
            logger.Info("Adding Series [{0}] Path: [{1}]", tvDbSeriesId, path);

            if (tvDbSeriesId <= 0)
            {
                throw new ArgumentOutOfRangeException("tvDbSeriesId", tvDbSeriesId.ToString());
            }

            var repoSeries = new Series();
            repoSeries.SeriesId = tvDbSeriesId;
            repoSeries.Path = path;
            repoSeries.Monitored = true; //New shows should be monitored
            repoSeries.QualityProfileId = qualityProfileId;
            repoSeries.Title = title;
            if (qualityProfileId == 0)
                repoSeries.QualityProfileId = _configProvider.DefaultQualityProfile;

            repoSeries.SeasonFolder = _configProvider.UseSeasonFolder;
            repoSeries.BacklogSetting = BacklogSettingType.Inherit;

            if (airedAfter.HasValue)
                repoSeries.CustomStartDate = airedAfter;

            _seriesRepository.Insert(repoSeries);
        }


        public virtual void UpdateFromSeriesEditor(IList<Series> editedSeries)
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
        private static string CleanAirsTime(string rawTime)
        {
            var match = TimeRegex.Match(rawTime);
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