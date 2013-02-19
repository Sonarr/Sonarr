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
    public class DiskScanJob : IJob
    {
        private readonly SeriesProvider _seriesProvider;
        private readonly DiskScanProvider _diskScanProvider;
        private readonly ConfigProvider _configProvider;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DiskScanJob(SeriesProvider seriesProvider, DiskScanProvider diskScanProvider,
                            ConfigProvider configProvider)
        {
            _seriesProvider = seriesProvider;
            _diskScanProvider = diskScanProvider;
            _configProvider = configProvider;
        }

        public DiskScanJob()
        {
        }

        public string Name
        {
            get { return "Media File Scan"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromHours(6); }
        }

        public virtual void Start(ProgressNotification notification, dynamic options)
        {
            IList<Series> seriesToScan;
            if (options == null || options.SeriesId == 0)
            {
                if (_configProvider.IgnoreArticlesWhenSortingSeries)
                    seriesToScan = _seriesProvider.All().OrderBy(o => o.Title.IgnoreArticles()).ToList();

                else
                    seriesToScan = _seriesProvider.All().OrderBy(o => o.Title).ToList();
            }
            else
            {
                seriesToScan = new List<Series>() { _seriesProvider.Get(options.SeriesId) };
            }

            foreach (var series in seriesToScan)
            {
                try
                {
                    notification.CurrentMessage = string.Format("Scanning disk for '{0}'", series.Title);
                    _diskScanProvider.Scan(series);
                    notification.CurrentMessage = string.Format("Disk Scan completed for '{0}'", series.Title);
                }
                catch (Exception e)
                {
                    Logger.ErrorException("An error has occurred while scanning " + series.Title, e);
                }
            }
        }
    }
}
