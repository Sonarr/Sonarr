using System.Linq;
using System;
using Ninject;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;

namespace NzbDrone.Core.Jobs
{
    public class DeleteSeriesJob : IJob
    {
        private readonly SeriesProvider _seriesProvider;
        private readonly DiskProvider _diskProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public DeleteSeriesJob(SeriesProvider seriesProvider, DiskProvider diskProvider)
        {
            _seriesProvider = seriesProvider;
            _diskProvider = diskProvider;
        }

        public string Name
        {
            get { return "Delete Series"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromTicks(0); }
        }

        public void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            DeleteSeries(notification, targetId, Convert.ToBoolean(secondaryTargetId));
        }

        private void DeleteSeries(ProgressNotification notification, int seriesId, bool deleteFiles)
        {
            Logger.Trace("Deleting Series [{0}]", seriesId);

            var series = _seriesProvider.GetSeries(seriesId);
            var title = series.Title;

            notification.CurrentMessage = String.Format("Deleting '{0}' from database", title);

            _seriesProvider.DeleteSeries(seriesId);

            notification.CurrentMessage = String.Format("Successfully deleted '{0}' from database", title);

            if (deleteFiles)
            {
                notification.CurrentMessage = String.Format("Deleting files from disk for series '{0}'", title);

                _diskProvider.DeleteFolder(series.Path, true);

                notification.CurrentMessage = String.Format("Successfully deleted files from disk for series '{0}'", title);
            }
        }
    }
}