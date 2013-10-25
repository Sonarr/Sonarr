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
        private readonly Logger _logger;


        public BlackholeProvider(IConfigService configService, IHttpProvider httpProvider, Logger logger)
        {
            _configService = configService;
            _httpProvider = httpProvider;
            _logger = logger;
        }

        public string DownloadNzb(RemoteEpisode remoteEpisode)
        {
            var url = remoteEpisode.Release.DownloadUrl;
            var title = remoteEpisode.Release.Title;

            title = FileNameBuilder.CleanFilename(title);

            var filename = Path.Combine(_configService.BlackholeFolder, title + ".nzb");


            _logger.Trace("Downloading NZB from: {0} to: {1}", url, filename);
            _httpProvider.DownloadFile(url, filename);
            _logger.Trace("NZB Download succeeded, saved to: {0}", filename);

            return null;
        }

        public bool IsConfigured
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_configService.BlackholeFolder);
            }
        }

        public IEnumerable<QueueItem> GetQueue()
        {
            return new QueueItem[0];
        }

        public IEnumerable<HistoryItem> GetHistory(int start = 0, int limit = 0)
        {
            return new HistoryItem[0];
        }

        public void RemoveFromQueue(string id)
        {
        }

        public void RemoveFromHistory(string id)
        {
        }
    }
}
