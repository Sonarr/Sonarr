using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.Clients.Pneumatic
{
    public class Pneumatic : DownloadClientBase<FolderSettings>, IExecute<TestPneumaticCommand>
    {
        private readonly IConfigService _configService;
        private readonly IHttpProvider _httpProvider;
        private readonly IDiskProvider _diskProvider;

        private static readonly Logger logger =  NzbDroneLogger.GetLogger();

        public Pneumatic(IConfigService configService, IHttpProvider httpProvider,
                                    IDiskProvider diskProvider)
        {
            _configService = configService;
            _httpProvider = httpProvider;
            _diskProvider = diskProvider;
        }

        public override string DownloadNzb(RemoteEpisode remoteEpisode)
        {
            var url = remoteEpisode.Release.DownloadUrl;
            var title = remoteEpisode.Release.Title;

            if (remoteEpisode.ParsedEpisodeInfo.FullSeason)
            {
                throw new NotImplementedException("Full season releases are not supported with Pneumatic.");
            }

            title = FileNameBuilder.CleanFilename(title);

            //Save to the Pneumatic directory (The user will need to ensure its accessible by XBMC)
            var filename = Path.Combine(Settings.Folder, title + ".nzb");

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
                return !string.IsNullOrWhiteSpace(Settings.Folder);
            }
        }

        public override IEnumerable<QueueItem> GetQueue()
        {
            return new QueueItem[0];
        }

        public override IEnumerable<HistoryItem> GetHistory(int start = 0, int limit = 10)
        {
            return new HistoryItem[0];
        }

        public override void RemoveFromQueue(string id)
        {
        }

        public override void RemoveFromHistory(string id)
        {
        }

        public override void Test()
        {
            PerformTest(Settings.Folder);
        }

        private void PerformTest(string folder)
        {
            var testPath = Path.Combine(folder, "drone_test.txt");
            _diskProvider.WriteAllText(testPath, DateTime.Now.ToString());
            _diskProvider.DeleteFile(testPath);
        }

        public void Execute(TestPneumaticCommand message)
        {
            PerformTest(message.Folder);
        }
    }
}
