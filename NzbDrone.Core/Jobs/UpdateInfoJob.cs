using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
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
        private readonly SeriesProvider _seriesProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly ReferenceDataProvider _referenceDataProvider;
        private readonly ConfigProvider _configProvider;
        private readonly ISeriesRepository _seriesRepository;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public UpdateInfoJob(SeriesProvider seriesProvider, EpisodeProvider episodeProvider,
                            ReferenceDataProvider referenceDataProvider, ConfigProvider configProvider, ISeriesRepository seriesRepository)
        {
            _seriesProvider = seriesProvider;
            _episodeProvider = episodeProvider;
            _referenceDataProvider = referenceDataProvider;
            _configProvider = configProvider;
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
            IList<Series> seriesToUpdate;
            if (options == null || options.SeriesId == 0)
            {
                if (_configProvider.IgnoreArticlesWhenSortingSeries)
                    seriesToUpdate = _seriesRepository.All().OrderBy(o => o.Title.IgnoreArticles()).ToList();

                else
                    seriesToUpdate = _seriesRepository.All().OrderBy(o => o.Title).ToList();
            }
            else
            {
                seriesToUpdate = new List<Series> { _seriesRepository.Get(options.SeriesId) };
            }

            //Update any Daily Series in the DB with the IsDaily flag
            _referenceDataProvider.UpdateDailySeries();

            foreach (var series in seriesToUpdate)
            {
                try
                {
                    notification.CurrentMessage = "Updating " + series.Title;
                    _seriesProvider.UpdateSeriesInfo(series.SeriesId);
                    _episodeProvider.RefreshEpisodeInfo(series);
                    notification.CurrentMessage = "Update completed for " + series.Title;
                }

                catch(Exception ex)
                {
                    Logger.ErrorException("Failed to update episode info for series: " + series.Title, ex);
                }
                
            }
        }
    }
}