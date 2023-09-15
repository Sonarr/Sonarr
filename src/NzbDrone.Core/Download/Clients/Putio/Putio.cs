using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NLog;
using FluentValidation.Results;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;

namespace NzbDrone.Core.Download.Clients.Putio
{
    public class Putio : TorrentClientBase<PutioSettings>
    {
        private readonly IPutioProxy _proxy;

        public Putio(IPutioProxy proxy,
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

        public override string Name
        {
            get
            {
                return "put.io";
            }
        }
        protected override string AddFromMagnetLink(RemoteEpisode remoteEpisode, string hash, string magnetLink)
        {
            _proxy.AddTorrentFromUrl(magnetLink, Settings);
            return hash;
        }

        protected override string AddFromTorrentFile(RemoteEpisode remoteEpisode, string hash, string filename, byte[] fileContent)
        {
            _proxy.AddTorrentFromData(fileContent, Settings);
            return hash;
        }

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            List<PutioTorrent> torrents;

            try
            {
                torrents = _proxy.GetTorrents(Settings);
            }
            catch (DownloadClientException ex)
            {
                _logger.Error(ex, ex.Message);
                return Enumerable.Empty<DownloadClientItem>();
            }

            var items = new List<DownloadClientItem>();

            foreach (var torrent in torrents)
            {
                // If totalsize == 0 the torrent is a magnet downloading metadata
                if (torrent.Size == 0)
                    continue;

                var item = new DownloadClientItem();
                item.DownloadId = "putio-" + torrent.Id;
                item.Category = Settings.SaveParentId;
                item.Title = torrent.Name;

                item.DownloadClient = Definition.Name;

                item.TotalSize = torrent.Size;
                item.RemainingSize = torrent.Size - torrent.Downloaded;

                try
                {
                    if (torrent.FileId != 0)
                    {
                        var file = _proxy.GetFile(torrent.FileId, Settings);
                        var torrentPath = "/completed/" + file.Name;

                        var outputPath = _remotePathMappingService.RemapRemoteToLocal(Settings.Url, new OsPath(torrentPath));

                        if (Settings.SaveParentId.IsNotNullOrWhiteSpace())
                        {
                            var directories = outputPath.FullPath.Split('\\', '/');
                            if (!directories.Contains(string.Format("{0}", Settings.SaveParentId))) continue;
                        }

                        item.OutputPath = outputPath; // + torrent.Name;
                    }
                }
                catch (DownloadClientException ex)
                {
                    _logger.Error(ex, ex.Message);
                }

                if (torrent.EstimatedTime >= 0)
                {
                    item.RemainingTime = TimeSpan.FromSeconds(torrent.EstimatedTime);
                }

                if (!torrent.ErrorMessage.IsNullOrWhiteSpace())
                {
                    item.Status = DownloadItemStatus.Warning;
                    item.Message = torrent.ErrorMessage;
                }
                else if (torrent.Status == PutioTorrentStatus.Completed)
                {
                    item.Status = DownloadItemStatus.Completed;
                }
                else if (torrent.Status == PutioTorrentStatus.InQueue)
                {
                    item.Status = DownloadItemStatus.Queued;
                }
                else
                {
                    item.Status = DownloadItemStatus.Downloading;
                }

                // item.IsReadOnly = torrent.Status != PutioTorrentStatus.Error;

                items.Add(item);
            }

            return items;
        }

        public override void RemoveItem(string downloadId, bool deleteData)
        {
            _proxy.RemoveTorrent(downloadId.ToLower(), Settings);
        }

        public override DownloadClientStatus GetStatus()
        {
            var destDir = string.Format("{0}", Settings.SaveParentId);

            return new DownloadClientStatus
            {
                IsLocalhost = false,
                OutputRootFolders = new List<OsPath> { _remotePathMappingService.RemapRemoteToLocal(Settings.Url, new OsPath(destDir)) }
            };
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
            if (failures.Any()) return;
            failures.AddIfNotNull(TestGetTorrents());
        }

        private ValidationFailure TestConnection()
        {
            try
            {
                _proxy.GetAccountSettings(Settings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return new NzbDroneValidationFailure(string.Empty, "Unknown exception: " + ex.Message);
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
                _logger.Error(ex, ex.Message);
                return new NzbDroneValidationFailure(string.Empty, "Failed to get the list of torrents: " + ex.Message);
            }

            return null;
        }
    }
}
