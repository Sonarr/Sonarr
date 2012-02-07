using System;
using System.IO;
using System.Linq;
using NLog;
using Ninject;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.DecisionEngine;

namespace NzbDrone.Core.Providers.DownloadClients
{
    public class BlackholeProvider : IDownloadClient
    {
        private readonly ConfigProvider _configProvider;
        private readonly HttpProvider _httpProvider;
        private readonly DiskProvider _diskProvider;
        private readonly UpgradeHistorySpecification _upgradeHistorySpecification;
        private readonly HistoryProvider _historyProvider;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public BlackholeProvider(ConfigProvider configProvider, HttpProvider httpProvider,
                                    DiskProvider diskProvider, UpgradeHistorySpecification upgradeHistorySpecification)
        {
            _configProvider = configProvider;
            _httpProvider = httpProvider;
            _diskProvider = diskProvider;
            _upgradeHistorySpecification = upgradeHistorySpecification;
        }

        public BlackholeProvider()
        {
        }

        public virtual bool DownloadNzb(string url, string title)
        {
            try
            {
                var filename = Path.Combine(_configProvider.BlackholeDirectory, title + ".nzb");

                if (_diskProvider.FileExists(filename))
                {
                    //Return true so a lesser quality is not returned.
                    logger.Info("NZB already exists on disk: {0)", filename);
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
