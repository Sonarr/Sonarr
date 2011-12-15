using System.Collections.Generic;
using System.Linq;
using Ninject;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Jobs
{
    public class UpdateInfoJob : IJob
    {
        private readonly SeriesProvider _seriesProvider;
        private readonly EpisodeProvider _episodeProvider;
        private readonly ReferenceDataProvider _referenceDataProvider;

        [Inject]
        public UpdateInfoJob(SeriesProvider seriesProvider, EpisodeProvider episodeProvider,
                            ReferenceDataProvider referenceDataProvider)
        {
            _seriesProvider = seriesProvider;
            _episodeProvider = episodeProvider;
            _referenceDataProvider = referenceDataProvider;
        }

        public UpdateInfoJob()
        {

        }

        public string Name
        {
            get { return "Update Episode Info"; }
        }

        public int DefaultInterval
        {
            get { return 720; } //12-hours
        }

        public virtual void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            IList<Series> seriesToUpdate;
            if (targetId == 0)
            {
                seriesToUpdate = _seriesProvider.GetAllSeries().OrderBy(o => SortHelper.SkipArticles(o.Title)).ToList();
            }
            else
            {
                seriesToUpdate = new List<Series>() { _seriesProvider.GetSeries(targetId) };
            }

            //Update any Daily Series in the DB with the IsDaily flag
            _referenceDataProvider.UpdateDailySeries();

            foreach (var series in seriesToUpdate)
            {
                notification.CurrentMessage = "Updating " + series.Title;
                _seriesProvider.UpdateSeriesInfo(series.SeriesId);
                _episodeProvider.RefreshEpisodeInfo(series);
                notification.CurrentMessage = "Update completed for " + series.Title;
            }
        }
    }
}