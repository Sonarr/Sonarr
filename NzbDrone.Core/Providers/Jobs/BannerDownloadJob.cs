using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ninject;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers.Jobs
{
    public class BannerDownloadJob : IJob
    {
        private readonly SeriesProvider _seriesProvider;
        private readonly HttpProvider _httpProvider;
        private readonly DiskProvider _diskProvider;
        private readonly EnviromentProvider _enviromentProvider;
        private readonly PathProvider _pathProvider;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string _bannerUrlPrefix = "http://www.thetvdb.com/banners/";

        [Inject]
        public BannerDownloadJob(SeriesProvider seriesProvider, HttpProvider httpProvider, DiskProvider diskProvider,
            EnviromentProvider enviromentProvider, PathProvider pathProvider)
        {
            _seriesProvider = seriesProvider;
            _httpProvider = httpProvider;
            _diskProvider = diskProvider;
            _enviromentProvider = enviromentProvider;
            _pathProvider = pathProvider;
        }

        public BannerDownloadJob()
        {
        }

        public string Name
        {
            get { return "Banner Download"; }
        }

        public int DefaultInterval
        {
            //30 days
            get { return 43200; }
        }

        public virtual void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            Logger.Debug("Starting banner download job");


            _diskProvider.CreateDirectory(_pathProvider.BannerPath);

            if (targetId > 0)
            {
                var series = _seriesProvider.GetSeries(targetId);

                if (series != null && !String.IsNullOrEmpty(series.BannerUrl))
                    DownloadBanner(notification, series);

                return;
            }

            var seriesInDb = _seriesProvider.GetAllSeries();

            foreach (var series in seriesInDb.Where(s => !String.IsNullOrEmpty(s.BannerUrl)))
            {
                DownloadBanner(notification, series);
            }

            Logger.Debug("Finished banner download job");
        }

        public virtual void DownloadBanner(ProgressNotification notification, Series series)
        {
            var bannerFilename = Path.Combine(_pathProvider.BannerPath, series.SeriesId.ToString()) + ".jpg";

            notification.CurrentMessage = string.Format("Downloading banner for '{0}'", series.Title);

            try
            {
                _httpProvider.DownloadFile(_bannerUrlPrefix + series.BannerUrl, bannerFilename);
                notification.CurrentMessage = string.Format("Successfully download banner for '{0}'", series.Title);
            }
            catch (Exception)
            {
                Logger.Debug("Failed to download banner for '{0}'", series.Title);
                notification.CurrentMessage = string.Format("Failed to download banner for '{0}'", series.Title);
            }
        }
    }
}
