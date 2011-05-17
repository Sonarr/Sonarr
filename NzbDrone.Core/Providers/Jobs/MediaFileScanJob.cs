using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers.Jobs
{
    public class MediaFileScanJob : IJob
    {
        private readonly SeriesProvider _seriesProvider;
        private readonly MediaFileProvider _mediaFileProvider;

        public MediaFileScanJob(SeriesProvider seriesProvider, MediaFileProvider mediaFileProvider)
        {
            _seriesProvider = seriesProvider;
            _mediaFileProvider = mediaFileProvider;
        }

        public string Name
        {
            get { return "Media File Scan"; }
        }

        public int DefaultInterval
        {
            get { return 60; }
        }

        public void Start(ProgressNotification notification, int targetId)
        {
            IList<Series> seriesToScan;
            if (targetId == 0)
            {
                seriesToScan = _seriesProvider.GetAllSeries().ToList();
            }
            else
            {
                seriesToScan = new List<Series>() { _seriesProvider.GetSeries(targetId) };
            }

            foreach (var series in seriesToScan)
            {
                notification.CurrentMessage = string.Format("Scanning disk for '{0}'", series.Title);
                _mediaFileProvider.Scan(series);
                notification.CurrentMessage = string.Format("Media File Scan completed for '{0}'", series.Title);
            }
        }
    }
}
