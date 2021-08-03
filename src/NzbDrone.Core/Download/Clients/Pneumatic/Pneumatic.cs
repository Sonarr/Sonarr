using System;
using System.Collections.Generic;
using System.IO;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;

namespace NzbDrone.Core.Download.Clients.Pneumatic
{
    public class Pneumatic : DownloadClientBase<PneumaticSettings>
    {
        private readonly IHttpClient _httpClient;

        public Pneumatic(IHttpClient httpClient,
                         IConfigService configService,
                         IDiskProvider diskProvider,
                         IRemotePathMappingService remotePathMappingService,
                         Logger logger)
            : base(configService, diskProvider, remotePathMappingService, logger)
        {
            _httpClient = httpClient;
        }

        public override string Name => "Pneumatic";

        public override DownloadProtocol Protocol => DownloadProtocol.Usenet;

        public override string Download(RemoteEpisode remoteEpisode)
        {
            var url = remoteEpisode.Release.DownloadUrl;
            var title = remoteEpisode.Release.Title;

            if (remoteEpisode.ParsedEpisodeInfo.FullSeason)
            {
                throw new NotSupportedException("Full season releases are not supported with Pneumatic.");
            }

            title = FileNameBuilder.CleanFileName(title);

            //Save to the Pneumatic directory (The user will need to ensure its accessible by XBMC)
            var nzbFile = Path.Combine(Settings.NzbFolder, title + ".nzb");

            _logger.Debug("Downloading NZB from: {0} to: {1}", url, nzbFile);
            _httpClient.DownloadFile(url, nzbFile);

            _logger.Debug("NZB Download succeeded, saved to: {0}", nzbFile);

            var strmFile = WriteStrmFile(title, nzbFile);

            return GetDownloadClientId(strmFile);
        }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(Settings.NzbFolder);

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            foreach (var file in _diskProvider.GetFiles(Settings.StrmFolder, SearchOption.TopDirectoryOnly))
            {
                if (Path.GetExtension(file) != ".strm")
                {
                    continue;
                }

                var title = FileNameBuilder.CleanFileName(Path.GetFileName(file));

                var historyItem = new DownloadClientItem
                {
                    DownloadClientInfo = DownloadClientItemClientInfo.FromDownloadClient(this),
                    DownloadId = GetDownloadClientId(file),
                    Title = title,

                    CanBeRemoved = true,
                    CanMoveFiles = true,

                    TotalSize = _diskProvider.GetFileSize(file),

                    OutputPath = new OsPath(file)
                };

                if (_diskProvider.IsFileLocked(file))
                {
                    historyItem.Status = DownloadItemStatus.Downloading;
                }
                else
                {
                    historyItem.Status = DownloadItemStatus.Completed;
                }

                yield return historyItem;
            }
        }

        public override void RemoveItem(DownloadClientItem item, bool deleteData)
        {
            throw new NotSupportedException();
        }

        public override DownloadClientInfo GetStatus()
        {
            var status = new DownloadClientInfo
            {
                IsLocalhost = true
            };

            return status;
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestFolder(Settings.NzbFolder, "NzbFolder"));
            failures.AddIfNotNull(TestFolder(Settings.StrmFolder, "StrmFolder"));
        }

        private string WriteStrmFile(string title, string nzbFile)
        {
            if (Settings.StrmFolder.IsNullOrWhiteSpace())
            {
                throw new DownloadClientException("Strm Folder needs to be set for Pneumatic Downloader");
            }

            var contents = string.Format("plugin://plugin.program.pneumatic/?mode=strm&type=add_file&nzb={0}&nzbname={1}", nzbFile, title);
            var filename = Path.Combine(Settings.StrmFolder, title + ".strm");

            _diskProvider.WriteAllText(filename, contents);

            return filename;
        }

        private string GetDownloadClientId(string filename)
        {
            return Definition.Name + "_" + Path.GetFileName(filename) + "_" + _diskProvider.FileGetLastWrite(filename).Ticks;
        }
    }
}
