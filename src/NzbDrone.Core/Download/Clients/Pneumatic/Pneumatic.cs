using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation.Results;
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
using Omu.ValueInjecter;

namespace NzbDrone.Core.Download.Clients.Pneumatic
{
    public class Pneumatic : DownloadClientBase<PneumaticSettings>
    {
        private readonly IHttpProvider _httpProvider;
        private readonly IDiskProvider _diskProvider;

        public Pneumatic(IHttpProvider httpProvider,
                         IDiskProvider diskProvider,
                         IConfigService configService,
                         IParsingService parsingService,
                         Logger logger)
            : base(configService, parsingService, logger)
        {
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

            _logger.Debug("Downloading NZB from: {0} to: {1}", url, filename);
            _httpProvider.DownloadFile(url, filename);

            _logger.Debug("NZB Download succeeded, saved to: {0}", filename);

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

        public override DownloadClientStatus GetStatus()
        {
            var status = new DownloadClientStatus
            {
                IsLocalhost = true
            };

            return status;
        }

        public override IEnumerable<ValidationFailure> Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(PerformWriteTest(Settings.NzbFolder, "NzbFolder"));

            return failures;
        }

        private ValidationFailure PerformWriteTest(String folder, String propertyName)
        {
            if (!_diskProvider.FolderExists(folder))
            {
                return new ValidationFailure(propertyName, "Folder does not exist");
            }

            try
            {
                var testPath = Path.Combine(folder, "drone_test.txt");
                _diskProvider.WriteAllText(testPath, DateTime.Now.ToString());
                _diskProvider.DeleteFile(testPath);
            }
            catch (Exception ex)
            {
                _logger.ErrorException(ex.Message, ex);
                return new ValidationFailure(propertyName, "Unable to write to folder");
            }

            return null;
        }
    }
}
