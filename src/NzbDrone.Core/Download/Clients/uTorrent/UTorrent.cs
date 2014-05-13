using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NLog;
using Omu.ValueInjecter;
using FluentValidation.Results;

namespace NzbDrone.Core.Download.Clients.UTorrent
{
    public class UTorrent : TorrentClientBase<UTorrentSettings>
    {
        private readonly IUTorrentProxy _proxy;

        public UTorrent(IUTorrentProxy proxy,
                        ITorrentFileInfoReader torrentFileInfoReader,
                        IHttpProvider httpProvider,
                        IConfigService configService,
                        IDiskProvider diskProvider,
                        IParsingService parsingService,
                        Logger logger)
            : base(httpProvider, torrentFileInfoReader, configService, diskProvider, parsingService, logger)
        {
            _proxy = proxy;
        }

        protected override String AddFromMagnetLink(String hash, String magnetLink)
        {
            _proxy.AddTorrentFromUrl(magnetLink, Settings);

            var torrents = _proxy.GetTorrents(Settings);
            
            _proxy.SetTorrentLabel(hash, Settings.TvCategory, Settings);

            return hash;
        }

        protected override String AddFromTorrentFile(String hash, String filename, Byte[] fileContent)
        {
            _proxy.AddTorrentFromFile(filename, fileContent, Settings);

            var torrents = _proxy.GetTorrents(Settings);

            _proxy.SetTorrentLabel(hash, Settings.TvCategory, Settings);

            return hash;
        }

        public override IEnumerable<DownloadClientItem> GetItems()
        {            
            List<UTorrentTorrent> torrents;

            try
            {
                torrents = _proxy.GetTorrents(Settings);
            }
            catch (DownloadClientException ex)
            {
                _logger.ErrorException(ex.Message, ex);
                return Enumerable.Empty<DownloadClientItem>();
            }

            var queueItems = new List<DownloadClientItem>();

            foreach (var torrent in torrents)
            {
                if (torrent.Label != Settings.TvCategory)
                {
                    continue;
                }

                var remoteEpisode = GetRemoteEpisode(torrent.Name);
                if (remoteEpisode == null || remoteEpisode.Series == null) continue;

                var item = new DownloadClientItem();
                item.DownloadClientId = torrent.Hash;
                item.Title = torrent.Name;
                item.TotalSize = torrent.Size;
                item.Category = torrent.Label;
                item.DownloadClient = Definition.Name;
                item.RemainingSize = torrent.Remaining;
                item.RemainingTime = TimeSpan.FromSeconds(torrent.Eta);
                item.RemoteEpisode = remoteEpisode;

                if (torrent.RootDownloadPath == null || Path.GetFileName(torrent.RootDownloadPath) == torrent.Name)
                {
                    item.OutputPath = torrent.RootDownloadPath;
                }
                else
                {
                    item.OutputPath = Path.Combine(torrent.RootDownloadPath, torrent.Name);
                }

                if (torrent.Status.HasFlag(UTorrentTorrentStatus.Error))
                {
                    item.Status = DownloadItemStatus.Failed;
                }
                else if (torrent.Status.HasFlag(UTorrentTorrentStatus.Loaded) && 
                         torrent.Status.HasFlag(UTorrentTorrentStatus.Checked) && torrent.Remaining == 0 && torrent.Progress == 1.0)
                {
                    item.Status = DownloadItemStatus.Completed;
                }
                else if (torrent.Status.HasFlag(UTorrentTorrentStatus.Paused))
                {
                    item.Status = DownloadItemStatus.Paused;
                }
                else if (torrent.Status.HasFlag(UTorrentTorrentStatus.Started))
                {
                    item.Status = DownloadItemStatus.Downloading;
                }
                else
                {
                    item.Status = DownloadItemStatus.Queued;
                }

                // 'Started' without 'Queued' is when the torrent is 'forced seeding'
                item.IsReadOnly = torrent.Status.HasFlag(UTorrentTorrentStatus.Queued) || torrent.Status.HasFlag(UTorrentTorrentStatus.Started);

                queueItems.Add(item);
            }

            return queueItems;
        }

        public override void RemoveItem(String id)
        {
            _proxy.RemoveTorrent(id, false, Settings);
        }

        public override String RetryDownload(String id)
        {
            throw new NotSupportedException();
        }

        public override DownloadClientStatus GetStatus()
        {
            var config = _proxy.GetConfig(Settings);

            String destDir = null;

            if (config.GetValueOrDefault("dir_active_download_flag") == "true")
            {
                destDir = config.GetValueOrDefault("dir_active_download");
            }

            if (config.GetValueOrDefault("dir_completed_download_flag") == "true")
            {
                destDir = config.GetValueOrDefault("dir_completed_download");

                if (config.GetValueOrDefault("dir_add_label") == "true")
                {
                    destDir = Path.Combine(destDir, Settings.TvCategory);
                }
            }

            var status = new DownloadClientStatus
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost"
            };

            if (destDir != null)
            {
                status.OutputRootFolders = new List<string> { destDir };
            }

            return status;
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
        }

        private ValidationFailure TestConnection()
        {
            try
            {
                var version = _proxy.GetVersion(Settings);

                if (version < 25406)
                {
                    return new ValidationFailure(string.Empty, "Old uTorrent client with unsupported API, need 3.0 or higher");
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException(ex.Message, ex);
                return new ValidationFailure("Host", "Unable to connect to uTorrent");
            }

            return null;
        }
    }
}
