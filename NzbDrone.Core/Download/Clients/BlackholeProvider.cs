using System;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Model;
using NzbDrone.Core.Organizer;

namespace NzbDrone.Core.Download.Clients
{
    public class BlackholeProvider : IDownloadClient
    {
        private readonly IConfigService _configService;
        private readonly HttpProvider _httpProvider;
        private readonly DiskProvider _diskProvider;
        private readonly UpgradeHistorySpecification _upgradeHistorySpecification;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public BlackholeProvider(IConfigService configService, HttpProvider httpProvider,
                                    DiskProvider diskProvider, UpgradeHistorySpecification upgradeHistorySpecification)
        {
            _configService = configService;
            _httpProvider = httpProvider;
            _diskProvider = diskProvider;
            _upgradeHistorySpecification = upgradeHistorySpecification;
        }

        public BlackholeProvider()
        {
        }

        public virtual bool DownloadNzb(string url, string title, bool recentlyAired)
        {
            try
            {
                title = FileNameBuilder.CleanFilename(title);

                var filename = Path.Combine(_configService.BlackholeDirectory, title + ".nzb");

                if (_diskProvider.FileExists(filename))
                {
                    //Return true so a lesser quality is not returned.
                    logger.Info("NZB already exists on disk: {0}", filename);
                    return true;
                }

                logger.Trace("Downloading NZB from: {0} to: {1}", url, filename);
                _httpProvider.DownloadFile(url, filename);

                logger.Trace("NZB Download succeeded, saved to: {0}", filename);
                return true;
            }
            catch (Exception ex)
            {
                logger.WarnException("Failed to download NZB: " + url, ex);
                return false;
            }
        }

        public virtual bool IsInQueue(EpisodeParseResult newParseResult)
        {
            return !_upgradeHistorySpecification.IsSatisfiedBy(newParseResult);
        }
    }
}
