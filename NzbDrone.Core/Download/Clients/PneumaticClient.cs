using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.Clients
{
    public class PneumaticClient : IDownloadClient
    {
        private readonly IConfigService _configService;
        private readonly IHttpProvider _httpProvider;
        private readonly IDiskProvider _diskProvider;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public PneumaticClient(IConfigService configService, IHttpProvider httpProvider,
                                    IDiskProvider diskProvider)
        {
            _configService = configService;
            _httpProvider = httpProvider;
            _diskProvider = diskProvider;
        }

        public virtual bool DownloadNzb(string url, string title)
        {
            try
            {
                //Todo: Allow full season releases
                if (Parser.Parser.ParseTitle(title).FullSeason)
                {
                    logger.Info("Skipping Full Season Release: {0}", title);
                    return false;
                }

                title = FileNameBuilder.CleanFilename(title);

                //Save to the Pneumatic directory (The user will need to ensure its accessible by XBMC)
                var filename = Path.Combine(_configService.PneumaticFolder, title + ".nzb");

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
                _diskProvider.WriteAllText(Path.Combine(_configService.DownloadedEpisodesFolder, title + ".strm"), contents);

                return true;
            }
            catch (Exception ex)
            {
                logger.WarnException("Failed to download NZB: " + url, ex);
                return false;
            }
        }

        public IEnumerable<QueueItem> GetQueue()
        {
            return new QueueItem[0];
        }

        public virtual bool IsInQueue(RemoteEpisode newEpisode)
        {
            return false;
        }
    }
}
