using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.Clients.Pneumatic
{
    public class Pneumatic : DownloadClientBase<PneumaticSettings>, IExecute<TestPneumaticCommand>
    {
        private readonly IConfigService _configService;
        private readonly IHttpProvider _httpProvider;
        private readonly IDiskProvider _diskProvider;

        private static readonly Logger logger =  NzbDroneLogger.GetLogger();

        public Pneumatic(IConfigService configService,
                         IHttpProvider httpProvider,
                         IDiskProvider diskProvider,
                         IParsingService parsingService,
                         Logger logger)
            : base(parsingService, logger)
        {
            _configService = configService;
            _httpProvider = httpProvider;
            _diskProvider = diskProvider;
        }

        public override DownloadProtocol Protocol
        {
            get
            {
                return DownloadProtocol.Usenet;
            }
        }

        public override string Download(RemoteEpisode remoteEpisode)
        {
            var url = remoteEpisode.Release.DownloadUrl;
            var title = remoteEpisode.Release.Title;

            if (remoteEpisode.ParsedEpisodeInfo.FullSeason)
            {
                throw new NotSupportedException("Full season releases are not supported with Pneumatic.");
            }

            title = FileNameBuilder.CleanFilename(title);

            //Save to the Pneumatic directory (The user will need to ensure its accessible by XBMC)
            var filename = Path.Combine(Settings.NzbFolder, title + ".nzb");

            logger.Debug("Downloading NZB from: {0} to: {1}", url, filename);
            _httpProvider.DownloadFile(url, filename);

            logger.Debug("NZB Download succeeded, saved to: {0}", filename);

            var contents = String.Format("plugin://plugin.program.pneumatic/?mode=strm&type=add_file&nzb={0}&nzbname={1}", filename, title);
            _diskProvider.WriteAllText(Path.Combine(_configService.DownloadedEpisodesFolder, title + ".strm"), contents);

            return null;
        }

        public bool IsConfigured
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Settings.NzbFolder);
            }
        }

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            return new DownloadClientItem[0];
        }
        
        public override void RemoveItem(string id)
        {
            throw new NotSupportedException();
        }

        public override void RetryDownload(string id)
        {
            throw new NotSupportedException();
        }

        public override void Test()
        {
            PerformTest(Settings.NzbFolder);
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
