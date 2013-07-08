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
    public class BlackholeProvider : IDownloadClient
    {
        private readonly IConfigService _configService;
        private readonly IHttpProvider _httpProvider;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;


        public BlackholeProvider(IConfigService configService, IHttpProvider httpProvider,
                                    IDiskProvider diskProvider, Logger logger)
        {
            _configService = configService;
            _httpProvider = httpProvider;
            _diskProvider = diskProvider;
            _logger = logger;
        }


        public bool IsInQueue(RemoteEpisode newEpisode)
        {
            throw new NotImplementedException();
        }

        public bool DownloadNzb(RemoteEpisode remoteEpisode)
        {
            var url = remoteEpisode.Report.NzbUrl;
            var title = remoteEpisode.Report.Title;

            try
            {
                title = FileNameBuilder.CleanFilename(title);

                var filename = Path.Combine(_configService.BlackholeFolder, title + ".nzb");

                if (_diskProvider.FileExists(filename))
                {
                    //Return true so a lesser quality is not returned.
                    _logger.Info("NZB already exists on disk: {0}", filename);
                    return true;
                }

                _logger.Trace("Downloading NZB from: {0} to: {1}", url, filename);
                _httpProvider.DownloadFile(url, filename);

                _logger.Trace("NZB Download succeeded, saved to: {0}", filename);
                return true;
            }
            catch (Exception ex)
            {
                _logger.WarnException("Failed to download NZB: " + url, ex);
                return false;
            }
        }

        public IEnumerable<QueueItem> GetQueue()
        {
            return new QueueItem[0];
        }
    }
}
