using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.Clients.Hadouken.Models;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Hadouken
{
    public class Hadouken : TorrentClientBase<HadoukenSettings>
    {
        private readonly IHadoukenProxy _proxy;

        public Hadouken(IHadoukenProxy proxy,
                        ITorrentFileInfoReader torrentFileInfoReader,
                        IHttpClient httpClient,
                        IConfigService configService,
                        IDiskProvider diskProvider,
                        IRemotePathMappingService remotePathMappingService,
                        Logger logger)
            : base(torrentFileInfoReader, httpClient, configService, diskProvider, remotePathMappingService, logger)
        {
            _proxy = proxy;
        }

        public override string Name => "Hadouken";

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            HadoukenTorrent[] torrents;

            try
            {
                torrents = _proxy.GetTorrents(Settings);
            }
            catch (DownloadClientException ex)
            {
                _logger.ErrorException(ex.Message, ex);
                return Enumerable.Empty<DownloadClientItem>();
            }

            var items = new List<DownloadClientItem>();

            foreach (var torrent in torrents)
            {
                if (Settings.Category.IsNotNullOrWhiteSpace() && torrent.Label != Settings.Category)
                {
                    continue;
                }

                var outputPath = _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(torrent.SavePath));
                var eta = TimeSpan.FromSeconds(0);

                if (torrent.DownloadRate > 0 && torrent.TotalSize > 0)
                {
                    eta = TimeSpan.FromSeconds(torrent.TotalSize / (double)torrent.DownloadRate);
                }

                var item = new DownloadClientItem
                {
                    DownloadClient = Definition.Name,
                    DownloadId = torrent.InfoHash.ToUpper(),
                    OutputPath = outputPath + torrent.Name,
                    RemainingSize = torrent.TotalSize - torrent.DownloadedBytes,
                    RemainingTime = eta,
                    Title = torrent.Name,
                    TotalSize = torrent.TotalSize
                };

                if (!string.IsNullOrEmpty(torrent.Error))
                {
                    item.Status = DownloadItemStatus.Warning;
                    item.Message = torrent.Error;
                }
                else if (torrent.IsFinished && torrent.State != HadoukenTorrentState.CheckingFiles)
                {
                    item.Status = DownloadItemStatus.Completed;
                }
                else if (torrent.State == HadoukenTorrentState.QueuedForChecking)
                {
                    item.Status = DownloadItemStatus.Queued;
                }
                else if (torrent.State == HadoukenTorrentState.Paused)
                {
                    item.Status = DownloadItemStatus.Paused;
                }
                else
                {
                    item.Status = DownloadItemStatus.Downloading;
                }

                if (torrent.IsFinished && torrent.State == HadoukenTorrentState.Paused)
                {
                    item.IsReadOnly = false;
                }
                else
                {
                    item.IsReadOnly = true;
                }

                items.Add(item);
            }

            return items;
        }

        public override void RemoveItem(string downloadId, bool deleteData)
        {
            if (deleteData)
            {
                _proxy.RemoveTorrentAndData(Settings, downloadId);
            }
            else
            {
                _proxy.RemoveTorrent(Settings, downloadId);
            }
        }

        public override DownloadClientStatus GetStatus()
        {
            var config = _proxy.GetConfig(Settings);
            var destDir = new OsPath(config.GetValueOrDefault("bittorrent.defaultSavePath") as string);

            var status = new DownloadClientStatus
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost"
            };

            if (!destDir.IsEmpty)
            {
                status.OutputRootFolders = new List<OsPath> { _remotePathMappingService.RemapRemoteToLocal(Settings.Host, destDir) };
            }

            return status;
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
            if (failures.Any()) return;
            failures.AddIfNotNull(TestGetTorrents());
        }

        protected override string AddFromMagnetLink(RemoteEpisode remoteEpisode, string hash, string magnetLink)
        {
            _proxy.AddTorrentUri(Settings, magnetLink);

            return hash.ToUpper();
        }

        protected override string AddFromTorrentFile(RemoteEpisode remoteEpisode, string hash, string filename, byte[] fileContent)
        {
            return _proxy.AddTorrentFile(Settings, fileContent).ToUpper();
        }

        private ValidationFailure TestConnection()
        {
            try
            {
                var sysInfo = _proxy.GetSystemInfo(Settings);
                var version = new Version(sysInfo.Versions["hadouken"]);

                if (version < new Version("5.1"))
                {
                    return new ValidationFailure(string.Empty, "Old Hadouken client with unsupported API, need 5.1 or higher");                    
                }
            }
            catch (DownloadClientAuthenticationException ex)
            {
                _logger.ErrorException(ex.Message, ex);

                return new NzbDroneValidationFailure("Password", "Authentication failed");
            }

            return null;
        }

        private ValidationFailure TestGetTorrents()
        {
            try
            {
                _proxy.GetTorrents(Settings);
            }
            catch (Exception ex)
            {
                _logger.ErrorException(ex.Message, ex);
                return new NzbDroneValidationFailure(String.Empty, "Failed to get the list of torrents: " + ex.Message);
            }

            return null;
        }
    }
}
