using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Jobs
{
    public class BannerDownloadJob : IJob
    {
        private readonly SeriesProvider _seriesProvider;
        private readonly BannerProvider _bannerProvider;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string BANNER_URL_PREFIX = "http://www.thetvdb.com/banners/";

        public BannerDownloadJob(SeriesProvider seriesProvider, BannerProvider bannerProvider)
        {
            _seriesProvider = seriesProvider;
            _bannerProvider = bannerProvider;
        }

        public BannerDownloadJob()
        {
        }

        public string Name
        {
            get { return "Banner Download"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromDays(30); }
        }

        public virtual void Start(ProgressNotification notification, dynamic options)
        {
            Logger.Debug("Starting banner download job");

            if (options != null)
            {
                Series series = _seriesProvider.Get(options.SeriesId);

                if (series != null && !String.IsNullOrEmpty(series.BannerUrl))
                {
                    DownloadBanner(notification, series);
                }

                return;
            }

            var seriesInDb = _seriesProvider.All();

            foreach (var series in seriesInDb.Where(s => !String.IsNullOrEmpty(s.BannerUrl)))
            {
                DownloadBanner(notification, series);
            }

            Logger.Debug("Finished banner download job");
        }

        public virtual void DownloadBanner(ProgressNotification notification, Series series)
        {
            notification.CurrentMessage = string.Format("Downloading banner for '{0}'", series.Title);

            if (_bannerProvider.Download(series))
                notification.CurrentMessage = string.Format("Successfully download banner for '{0}'", series.Title);

            else
                notification.CurrentMessage = string.Format("Failed to download banner for '{0}'", series.Title);
        }
    }
}
