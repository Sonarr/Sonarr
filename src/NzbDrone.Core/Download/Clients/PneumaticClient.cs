using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Instrumentation;
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

        private static readonly Logger logger =  NzbDroneLogger.GetLogger();

        public PneumaticClient(IConfigService configService, IHttpProvider httpProvider,
                                    IDiskProvider diskProvider)
        {
            _configService = configService;
            _httpProvider = httpProvider;
            _diskProvider = diskProvider;
        }

        public string DownloadNzb(RemoteEpisode remoteEpisode)
        {
            var url = remoteEpisode.Release.DownloadUrl;
            var title = remoteEpisode.Release.Title;

            if (remoteEpisode.ParsedEpisodeInfo.FullSeason)
            {
                throw new NotImplementedException("Full season Pneumatic releases are not supported.");
            }

            title = FileNameBuilder.CleanFilename(title);

            //Save to the Pneumatic directory (The user will need to ensure its accessible by XBMC)
            var filename = Path.Combine(_configService.PneumaticFolder, title + ".nzb");

            logger.Trace("Downloading NZB from: {0} to: {1}", url, filename);
            _httpProvider.DownloadFile(url, filename);

            logger.Trace("NZB Download succeeded, saved to: {0}", filename);

            var contents = String.Format("plugin://plugin.program.pneumatic/?mode=strm&type=add_file&nzb={0}&nzbname={1}", filename, title);
            _diskProvider.WriteAllText(Path.Combine(_configService.DownloadedEpisodesFolder, title + ".strm"), contents);

            return null;
        }

        public bool IsConfigured
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_configService.PneumaticFolder);
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
