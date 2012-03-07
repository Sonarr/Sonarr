using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers
{
    public class BannerProvider
    {
        private readonly HttpProvider _httpProvider;
        private readonly EnvironmentProvider _environmentProvider;
        private readonly DiskProvider _diskProvider;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const string BANNER_URL_PREFIX = "http://www.thetvdb.com/banners/";

        public BannerProvider(HttpProvider httpProvider, EnvironmentProvider environmentProvider,
                                DiskProvider diskProvider)
        {
            _httpProvider = httpProvider;
            _environmentProvider = environmentProvider;
            _diskProvider = diskProvider;
        }

        public BannerProvider()
        {
            
        }

        public virtual bool Download(ProgressNotification notification, Series series)
        {
            var bannerPath = _environmentProvider.GetBannerPath();
            
            logger.Trace("Ensuring Banner Folder exists: ", bannerPath);
            _diskProvider.CreateDirectory(bannerPath);

            var bannerFilename = Path.Combine(bannerPath, series.SeriesId.ToString()) + ".jpg";

            notification.CurrentMessage = string.Format("Downloading banner for '{0}'", series.Title);
            logger.Trace("Downloading banner for '{0}'", series.Title);

            try
            {
                _httpProvider.DownloadFile(BANNER_URL_PREFIX + series.BannerUrl, bannerFilename);
                notification.CurrentMessage = string.Format("Successfully download banner for '{0}'", series.Title);
                logger.Trace("Successfully download banner for '{0}'", series.Title);
            }
            catch (Exception)
            {
                logger.Debug("Failed to download banner for '{0}'", series.Title);
                notification.CurrentMessage = string.Format("Failed to download banner for '{0}'", series.Title);
                return false;
            }

            return true;
        }

        public virtual bool Delete(int seriesId)
        {
            var bannerPath = _environmentProvider.GetBannerPath();
            var bannerFilename = Path.Combine(bannerPath, seriesId.ToString()) + ".jpg";

            try
            {
                logger.Trace("Checking if banner exists: {0}", bannerFilename);

                if (_diskProvider.FileExists(bannerFilename))
                {
                    logger.Trace("Deleting existing banner: {0}", bannerFilename);
                    _diskProvider.DeleteFile(bannerFilename);
                }
            }
            catch(Exception ex)
            {
                logger.WarnException("Failed to delete banner: " + bannerFilename, ex);
                return false;
            }
            return true;
        }
    }
}
