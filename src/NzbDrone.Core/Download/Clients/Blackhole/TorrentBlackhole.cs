using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Download.Clients.Blackhole
{
    public class TorrentBlackhole : TorrentClientBase<TorrentBlackholeSettings>
    {
        private readonly IScanWatchFolder _scanWatchFolder;

        public TimeSpan ScanGracePeriod { get; set; }

        public override bool PreferTorrentFile => true;

        public TorrentBlackhole(IScanWatchFolder scanWatchFolder,
                                ITorrentFileInfoReader torrentFileInfoReader,
                                IHttpClient httpClient,
                                IConfigService configService,
                                IDiskProvider diskProvider,
                                IRemotePathMappingService remotePathMappingService,
                                Logger logger)
            : base(torrentFileInfoReader, httpClient, configService, diskProvider, remotePathMappingService, logger)
        {
            _scanWatchFolder = scanWatchFolder;

            ScanGracePeriod = TimeSpan.FromSeconds(30);
        }

        protected override string AddFromMagnetLink(RemoteEpisode remoteEpisode, string hash, string magnetLink)
        {
            if (!Settings.SaveMagnetFiles)
            {
                throw new NotSupportedException("Blackhole does not support magnet links.");
            }

            var title = remoteEpisode.Release.Title;

            title = FileNameBuilder.CleanFileName(title);

            var filepath = Path.Combine(Settings.TorrentFolder, string.Format("{0}.magnet", title));

            var fileContent = Encoding.UTF8.GetBytes(magnetLink);
            using (var stream = _diskProvider.OpenWriteStream(filepath))
            {
                stream.Write(fileContent, 0, fileContent.Length);
            }

            _logger.Debug("Saving magnet link succeeded, saved to: {0}", filepath);

            return null;
        }

        protected override string AddFromTorrentFile(RemoteEpisode remoteEpisode, string hash, string filename, byte[] fileContent)
        {
            var title = remoteEpisode.Release.Title;

            title = FileNameBuilder.CleanFileName(title);

            var filepath = Path.Combine(Settings.TorrentFolder, string.Format("{0}.torrent", title));

            using (var stream = _diskProvider.OpenWriteStream(filepath))
            {
                stream.Write(fileContent, 0, fileContent.Length);
            }

            _logger.Debug("Torrent Download succeeded, saved to: {0}", filepath);

            return null;
        }

        public override string Name => "Torrent Blackhole";

        public override ProviderMessage Message => new ProviderMessage("Magnet links are not supported.", ProviderMessageType.Warning);


        public override IEnumerable<DownloadClientItem> GetItems()
        {
            foreach (var item in _scanWatchFolder.GetItems(Settings.WatchFolder, ScanGracePeriod))
            {
                yield return new DownloadClientItem
                {
                    DownloadClient = Definition.Name,
                    DownloadId = Definition.Name + "_" + item.DownloadId,
                    Category = "sonarr",
                    Title = item.Title,

                    TotalSize = item.TotalSize,
                    RemainingTime = item.RemainingTime,

                    OutputPath = item.OutputPath,

                    Status = item.Status,

                    IsReadOnly = Settings.ReadOnly
                };
            }
        }

        public override void RemoveItem(string downloadId, bool deleteData)
        {
            if (!deleteData)
            {
                throw new NotSupportedException("Blackhole cannot remove DownloadItem without deleting the data as well, ignoring.");
            }

            DeleteItemData(downloadId);
        }

        public override DownloadClientStatus GetStatus()
        {
            return new DownloadClientStatus
            {
                IsLocalhost = true,
                OutputRootFolders = new List<OsPath> { new OsPath(Settings.WatchFolder) }
            };
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestFolder(Settings.TorrentFolder, "TorrentFolder"));
            failures.AddIfNotNull(TestFolder(Settings.WatchFolder, "WatchFolder"));
        }
    }
}
