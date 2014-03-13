using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.Clients.Blackhole
{
    public class Blackhole : DownloadClientBase<FolderSettings>, IExecute<TestBlackholeCommand>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IHttpProvider _httpProvider;
        private readonly Logger _logger;

        public Blackhole(IDiskProvider diskProvider, IHttpProvider httpProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _httpProvider = httpProvider;
            _logger = logger;
        }

        public override string DownloadNzb(RemoteEpisode remoteEpisode)
        {
            var url = remoteEpisode.Release.DownloadUrl;
            var title = remoteEpisode.Release.Title;

            title = FileNameBuilder.CleanFilename(title);

            var filename = Path.Combine(Settings.Folder, title + ".nzb");


            _logger.Debug("Downloading NZB from: {0} to: {1}", url, filename);
            _httpProvider.DownloadFile(url, filename);
            _logger.Debug("NZB Download succeeded, saved to: {0}", filename);

            return null;
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

        public void Execute(TestBlackholeCommand message)
        {
            PerformTest(message.Folder);
        }
    }
}
