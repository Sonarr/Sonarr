using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NLog;
using FluentValidation.Results;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Validation;
using System.Net;
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
            _proxy.AddTorrentFromUrl(magnetLink, GetDownloadDirectory(), Settings);
            return hash;
        }

        protected override string AddFromTorrentFile(RemoteEpisode remoteEpisode, string hash, string filename, byte[] fileContent)
        {
            _proxy.AddTorrentFromData(fileContent, GetDownloadDirectory(), Settings);
            return hash;
        }

        private string GetDownloadDirectory()
        {
            return string.Format("{0}", Settings.SaveParentId);
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
                _logger.ErrorException(ex.Message, ex);
                return Enumerable.Empty<DownloadClientItem>();
            }

            var items = new List<DownloadClientItem>();

            foreach (var torrent in torrents)
            {
                // If totalsize == 0 the torrent is a magnet downloading metadata
                if (torrent.TotalSize == 0)
                    continue;

                var outputPath = _remotePathMappingService.RemapRemoteToLocal(Settings.Url, new OsPath(torrent.DownloadDir));

                if (Settings.SaveParentId.IsNotNullOrWhiteSpace())
                {
                    var directories = outputPath.FullPath.Split('\\', '/');
                    if (!directories.Contains(string.Format("{0}", Settings.SaveParentId))) continue;
                }

                var item = new DownloadClientItem();
                item.DownloadId = torrent.HashString.ToUpper();
                item.Category = Settings.SaveParentId;
                item.Title = torrent.Name;

                item.DownloadClient = Definition.Name;

                item.OutputPath = outputPath + torrent.Name;
                item.TotalSize = torrent.TotalSize;
                item.RemainingSize = torrent.LeftUntilDone;
                if (torrent.Eta >= 0)
                {
                    item.RemainingTime = TimeSpan.FromSeconds(torrent.Eta);
                }

                if (!torrent.ErrorString.IsNullOrWhiteSpace())
                {
                    item.Status = DownloadItemStatus.Warning;
                    item.Message = torrent.ErrorString;
                }
                else if (torrent.Status == PutioTorrentStatus.Seeding || torrent.Status == PutioTorrentStatus.SeedingWait)
                {
                    item.Status = DownloadItemStatus.Completed;
                }
                else if (torrent.IsFinished && torrent.Status != PutioTorrentStatus.Check && torrent.Status != PutioTorrentStatus.CheckWait)
                {
                    item.Status = DownloadItemStatus.Completed;
                }
                else if (torrent.Status == PutioTorrentStatus.Queued)
                {
                    item.Status = DownloadItemStatus.Queued;
                }
                else
                {
                    item.Status = DownloadItemStatus.Downloading;
                }

                item.IsReadOnly = torrent.Status != PutioTorrentStatus.Stopped;

                items.Add(item);
            }

            return items;
        }

        public override void RemoveItem(string downloadId, bool deleteData)
        {
            _proxy.RemoveTorrent(downloadId.ToLower(), deleteData, Settings);
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
                _logger.ErrorException(ex.Message, ex);
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
                _logger.ErrorException(ex.Message, ex);
                return new NzbDroneValidationFailure(string.Empty, "Failed to get the list of torrents: " + ex.Message);
            }

            return null;
        }
    }
}
