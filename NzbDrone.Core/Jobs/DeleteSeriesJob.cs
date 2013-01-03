using System.Linq;
using System;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;

namespace NzbDrone.Core.Jobs
{
    public class DeleteSeriesJob : IJob
    {
        private readonly SeriesProvider _seriesProvider;
        private readonly RecycleBinProvider _recycleBinProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DeleteSeriesJob(SeriesProvider seriesProvider, RecycleBinProvider recycleBinProvider)
        {
            _seriesProvider = seriesProvider;
            _recycleBinProvider = recycleBinProvider;
        }

        public string Name
        {
            get { return "Delete Series"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromTicks(0); }
        }

        public void Start(ProgressNotification notification, dynamic options)
        {
            if (options == null)
                throw new ArgumentNullException("options");

            if (options.SeriesId == 0)
                throw new ArgumentNullException("options.SeriesId");

            DeleteSeries(notification, options.SeriesId, options.DeleteFiles);
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

                _recycleBinProvider.DeleteDirectory(series.Path);

                notification.CurrentMessage = String.Format("Successfully deleted files from disk for series '{0}'", title);
            }
        }
    }
}