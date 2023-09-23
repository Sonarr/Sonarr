using System;
using System.Collections.Generic;
using System.Linq;
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
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Putio
{
    public class Putio : TorrentClientBase<PutioSettings>
    {
        private readonly IPutioProxy _proxy;

        public Putio(
            IPutioProxy proxy,
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
            Dictionary<string, PutioTorrentMetadata> metadata;

            try
            {
                torrents = _proxy.GetTorrents(Settings);
                metadata = _proxy.GetAllTorrentMetadata(Settings);
            }
            catch (DownloadClientException ex)
            {
                _logger.Error(ex, ex.Message);
                yield break;
            }

            foreach (var torrent in torrents)
            {
                if (torrent.Size == 0)
                {
                    // If totalsize == 0 the torrent is a magnet downloading metadata
                    continue;
                }

                if (Settings.SaveParentId.IsNotNullOrWhiteSpace() && torrent.SaveParentId != long.Parse(Settings.SaveParentId))
                {
                    // torrent is not related to our parent folder
                    continue;
                }

                var item = new DownloadClientItem
                {
                    DownloadId = torrent.Id.ToString(),
                    Category = Settings.SaveParentId?.ToString(),
                    Title = torrent.Name,
                    TotalSize = torrent.Size,
                    RemainingSize = torrent.Size - torrent.Downloaded,
                    DownloadClientInfo = DownloadClientItemClientInfo.FromDownloadClient(this),
                    SeedRatio = torrent.Ratio,

                    // Initial status, might change later
                    Status = GetDownloadItemStatus(torrent)
                };

                try
                {
                    if (torrent.FileId != 0)
                    {
                        // How needs the output path need to look if we have remote files?

                        // check if we need to download the torrent from the remote
                        var title = FileNameBuilder.CleanFileName(torrent.Name);

                        // _diskProvider.FileExists(new OsPath())
                        /*
                        var file = _proxy.GetFile(torrent.FileId, Settings);
                        var torrentPath = "/completed/" + file.Name;

                        var outputPath = _remotePathMappingService.RemapRemoteToLocal(Settings.Url, new OsPath(torrentPath));

                        if (Settings.SaveParentId.IsNotNullOrWhiteSpace())
                        {
                            var directories = outputPath.FullPath.Split('\\', '/');
                            if (!directories.Contains(string.Format("{0}", Settings.SaveParentId)))
                            {
                                continue;
                            }
                        }

                        item.OutputPath = outputPath; // + torrent.Name;
                        */
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

                yield return item;
            }
        }

        private DownloadItemStatus GetDownloadItemStatus(PutioTorrent torrent)
        {
            if (torrent.Status == PutioTorrentStatus.Completed ||
                torrent.Status == PutioTorrentStatus.Seeding)
            {
                return DownloadItemStatus.Completed;
            }

            if (torrent.Status == PutioTorrentStatus.InQueue ||
                torrent.Status == PutioTorrentStatus.Waiting ||
                torrent.Status == PutioTorrentStatus.PrepareDownload)
            {
                return DownloadItemStatus.Queued;
            }

            if (torrent.Status == PutioTorrentStatus.Error)
            {
                return DownloadItemStatus.Failed;
            }

            return DownloadItemStatus.Downloading;
        }

        public override DownloadClientInfo GetStatus()
        {
            var destDir = new OsPath(Settings.DownloadPath);

            return new DownloadClientInfo
            {
                IsLocalhost = false,
                OutputRootFolders = new List<OsPath> { destDir }
            };
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestFolder(Settings.DownloadPath, "DownloadPath"));
            failures.AddIfNotNull(TestConnection());
            if (failures.Any())
            {
                return;
            }

            failures.AddIfNotNull(TestGetTorrents());
        }

        private ValidationFailure TestConnection()
        {
            try
            {
                _proxy.GetAccountSettings(Settings);
            }
            catch (DownloadClientAuthenticationException ex)
            {
                _logger.Error(ex, ex.Message);
                return new NzbDroneValidationFailure("OAuthToken", "Authentication failed")
                {
                    DetailedDescription = "See the wiki for more details on how to obtain an OAuthToken"
                };
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

        public override void RemoveItem(DownloadClientItem item, bool deleteData)
        {
            throw new NotImplementedException();
        }

        public override void MarkItemAsImported(DownloadClientItem downloadClientItem)
        {
            // What to do here? Maybe delete the file and transfer from put.io?
            base.MarkItemAsImported(downloadClientItem);
        }
    }
}
