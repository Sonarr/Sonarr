using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.ReferenceData;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Jobs
{
    public class UpdateInfoJob : IJob
    {
        private readonly ISeriesService _seriesService;
        private readonly IEpisodeService _episodeService;
        private readonly DailySeriesService _dailySeriesService;
        private readonly IConfigService _configService;
        private readonly ISeriesRepository _seriesRepository;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public UpdateInfoJob(ISeriesService seriesService, IEpisodeService episodeService,
                            DailySeriesService dailySeriesService, IConfigService configService, ISeriesRepository seriesRepository)
        {
            _seriesService = seriesService;
            _episodeService = episodeService;
            _dailySeriesService = dailySeriesService;
            _configService = configService;
            _seriesRepository = seriesRepository;
        }

        public UpdateInfoJob()
        {

        }

        public string Name
        {
            get { return "Update Episode Info"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromHours(12); }
        }

        public virtual void Start(ProgressNotification notification, dynamic options)
        {
            IList<Series> ListOfSeriesToUpdate;
            if (options == null || options.SeriesId == 0)
            {
                if (_configService.IgnoreArticlesWhenSortingSeries)
                    ListOfSeriesToUpdate = _seriesRepository.All().OrderBy(o => o.Title.IgnoreArticles()).ToList();

                else
                    ListOfSeriesToUpdate = _seriesRepository.All().OrderBy(o => o.Title).ToList();
            }
            else
            {
                ListOfSeriesToUpdate = new List<Series>
                    {
                            _seriesRepository.Get((int)options.SeriesId)
                    };
            }

            //Update any Daily Series in the DB with the IsDaily flag
            _dailySeriesService.UpdateDailySeries();

            foreach (var seriesToUpdate in ListOfSeriesToUpdate)
            {
                var series = seriesToUpdate;

                try
                {
                    notification.CurrentMessage = "Updating " + series.Title;
                    series = _seriesService.UpdateSeriesInfo(series.Id);
                    _episodeService.RefreshEpisodeInfo(series);
                    notification.CurrentMessage = "Update completed for " + series.Title;
                }

                catch (Exception ex)
                {
                    Logger.ErrorException("Failed to update episode info for series: " + series.Title, ex);
                }

            }
        }
    }
}