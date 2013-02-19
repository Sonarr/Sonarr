using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Tv;
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

        public virtual bool Download(Series series)
        {
            var bannerPath = _environmentProvider.GetBannerPath();

            logger.Trace("Ensuring Banner Folder exists: ", bannerPath);
            _diskProvider.CreateDirectory(bannerPath);

            var bannerFilename = Path.Combine(bannerPath, series.SeriesId.ToString()) + ".jpg";

            logger.Trace("Downloading banner for '{0}'", series.Title);

            try
            {
                _httpProvider.DownloadFile(BANNER_URL_PREFIX + series.BannerUrl, bannerFilename);
                logger.Trace("Successfully download banner for '{0}'", series.Title);
            }
            catch (Exception)
            {
                logger.Debug("Failed to download banner for '{0}'", series.Title);
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

        public virtual void Download(string remotePath, string filename)
        {
            var url = BANNER_URL_PREFIX + remotePath;

            try
            {
                _httpProvider.DownloadFile(url, filename);
                logger.Trace("Successfully download banner from '{0}' to '{1}'", url, filename);
            }
            catch (Exception ex)
            {
                var message = String.Format("Failed to download Banner from '{0}' to '{1}'", url, filename);
                logger.DebugException(message, ex);
                throw;
            }
        }
    }
}
