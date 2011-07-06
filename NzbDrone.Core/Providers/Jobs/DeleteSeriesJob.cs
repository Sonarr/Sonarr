using System;
using Ninject;
using NLog;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Providers.Jobs
{
    public class DeleteSeriesJob : IJob
    {
        private readonly SeriesProvider _seriesProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public DeleteSeriesJob(SeriesProvider seriesProvider)
        {
            _seriesProvider = seriesProvider;
        }

        public string Name
        {
            get { return "Delete Series"; }
        }

        public int DefaultInterval
        {
            get { return 0; }
        }

        public void Start(ProgressNotification notification, int targetId)
        {
            DeleteSeries(notification, targetId);
        }

        private void DeleteSeries(ProgressNotification notification, int seriesId)
        {
            Logger.Warn("Deleting Series [{0}]", seriesId);

            try
            {
                var title = _seriesProvider.GetSeries(seriesId).Title;

                notification.CurrentMessage = String.Format("Deleting '{0}' from database", title);

                _seriesProvider.DeleteSeries(seriesId);

                notification.CurrentMessage = String.Format("Successfully deleted '{0}' from database", title);
            }
            catch (Exception e)
            {
                Logger.ErrorException("An error has occurred while deleting series: " + seriesId, e);
                throw;
            }
        }
    }
}