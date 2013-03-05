using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Model;
using NzbDrone.Core.DecisionEngine;

namespace NzbDrone.Core.Download.Clients
{
    public class PneumaticProvider : IDownloadClient
    {
        private readonly IConfigService _configService;
        private readonly HttpProvider _httpProvider;
        private readonly DiskProvider _diskProvider;
        private readonly UpgradeHistorySpecification _upgradeHistorySpecification;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public PneumaticProvider(IConfigService configService, HttpProvider httpProvider,
                                    DiskProvider diskProvider, UpgradeHistorySpecification upgradeHistorySpecification)
        {
            _configService = configService;
            _httpProvider = httpProvider;
            _diskProvider = diskProvider;
            _upgradeHistorySpecification = upgradeHistorySpecification;
        }

        public PneumaticProvider()
        {
        }

        public virtual bool DownloadNzb(string url, string title, bool recentlyAired)
        {
            try
            {
                //Todo: Allow full season releases
                if (Parser.ParseTitle(title).FullSeason)
                {
                    logger.Info("Skipping Full Season Release: {0}", title);
                    return false;
                }

                title = MediaFileService.CleanFilename(title);

                //Save to the Pneumatic directory (The user will need to ensure its accessible by XBMC)
                var filename = Path.Combine(_configService.PneumaticDirectory, title + ".nzb");

                if (_diskProvider.FileExists(filename))
                {
                    //Return true so a lesser quality is not returned.
                    logger.Info("NZB already exists on disk: {0}", filename);
                    return true;
                }

                logger.Trace("Downloading NZB from: {0} to: {1}", url, filename);
                _httpProvider.DownloadFile(url, filename);

                logger.Trace("NZB Download succeeded, saved to: {0}", filename);

                var contents = String.Format("plugin://plugin.program.pneumatic/?mode=strm&type=add_file&nzb={0}&nzbname={1}", filename, title);
                _diskProvider.WriteAllText(Path.Combine(_configService.DownloadClientTvDirectory, title + ".strm"), contents);
                
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
