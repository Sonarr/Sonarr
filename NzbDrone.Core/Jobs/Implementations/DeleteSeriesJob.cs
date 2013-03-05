using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Jobs.Implementations
{
    public class DeleteSeriesJob : IJob
    {
        private readonly ISeriesService _seriesService;
        private readonly RecycleBinProvider _recycleBinProvider;
        private readonly ISeriesRepository _seriesRepository;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DeleteSeriesJob(ISeriesService seriesService, RecycleBinProvider recycleBinProvider, ISeriesRepository seriesRepository)
        {
            _seriesService = seriesService;
            _recycleBinProvider = recycleBinProvider;
            _seriesRepository = seriesRepository;
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

            DeleteSeries(notification, (int)options.SeriesId, (bool)options.DeleteFiles);
        }

        private void DeleteSeries(ProgressNotification notification, int seriesId, bool deleteFiles)
        {
            Logger.Trace("Deleting Series [{0}]", seriesId);

            var series = _seriesRepository.Get(seriesId);
            var title = series.Title;

            notification.CurrentMessage = String.Format("Deleting '{0}' from database", title);

            _seriesRepository.Delete(seriesId);

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