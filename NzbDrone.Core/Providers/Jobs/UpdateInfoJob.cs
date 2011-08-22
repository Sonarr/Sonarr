using System.Collections.Generic;
using System.Linq;
using Ninject;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers.Jobs
{
    public class UpdateInfoJob : IJob
    {
        private readonly SeriesProvider _seriesProvider;
        private readonly EpisodeProvider _episodeProvider;

        [Inject]
        public UpdateInfoJob(SeriesProvider seriesProvider, EpisodeProvider episodeProvider)
        {
            _seriesProvider = seriesProvider;
            _episodeProvider = episodeProvider;
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
            get { return 1440; } //Daily
        }

        public virtual void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            IList<Series> seriesToUpdate;
            if (targetId == 0)
            {
                seriesToUpdate = _seriesProvider.GetAllSeries().ToList();
            }
            else
            {
                seriesToUpdate = new List<Series>() { _seriesProvider.GetSeries(targetId) };
            }

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