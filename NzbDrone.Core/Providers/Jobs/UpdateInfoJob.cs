using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers.Jobs
{
    public class UpdateInfoJob : IJob
    {
        private readonly SeriesProvider _seriesProvider;
        private readonly EpisodeProvider _episodeProvider;

        public UpdateInfoJob(SeriesProvider seriesProvider, EpisodeProvider episodeProvider)
        {
            _seriesProvider = seriesProvider;
            _episodeProvider = episodeProvider;
        }

        public string Name
        {
            get { return "Update Info"; }
        }

        public int DefaultInterval
        {
            get { return 1440; } //Daily
        }

        public void Start(ProgressNotification notification, int targetId)
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
                notification.CurrentStatus = "Updating series info for " + series.Title;
                _seriesProvider.UpdateSeriesInfo(series.SeriesId);
                notification.CurrentStatus = "Updating episode info for " + series.Title;
                _episodeProvider.RefreshEpisodeInfo(series.SeriesId);
            }
        }
    }
}