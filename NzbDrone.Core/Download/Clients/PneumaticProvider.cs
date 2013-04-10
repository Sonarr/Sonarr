using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Model;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Organizer;

namespace NzbDrone.Core.Download.Clients
{
    public class PneumaticProvider : IDownloadClient
    {
        private readonly IConfigService _configService;
        private readonly IHttpProvider _httpProvider;
        private readonly DiskProvider _diskProvider;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public PneumaticProvider(IConfigService configService, IHttpProvider httpProvider,
                                    DiskProvider diskProvider)
        {
            _configService = configService;
            _httpProvider = httpProvider;
            _diskProvider = diskProvider;
        }

        public virtual bool DownloadNzb(string url, string title, bool recentlyAired)
        {
            try
            {
                //Todo: Allow full season releases
                if (Parser.ParseTitle<ParseResult>(title).FullSeason)
                {
                    logger.Info("Skipping Full Season Release: {0}", title);
                    return false;
                }

                title = FileNameBuilder.CleanFilename(title);

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

        public virtual bool IsInQueue(IndexerParseResult newParseResult)
        {
            return false;
        }
    }
}
