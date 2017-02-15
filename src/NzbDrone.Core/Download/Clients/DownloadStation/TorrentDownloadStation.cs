using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.Clients.DownloadStation.Proxies;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.DownloadStation
{
    public class TorrentDownloadStation : TorrentClientBase<DownloadStationSettings>
    {
        protected readonly IDownloadStationProxy _proxy;
        protected readonly ISharedFolderResolver _sharedFolderResolver;
        protected readonly ISerialNumberProvider _serialNumberProvider;

        public TorrentDownloadStation(IDownloadStationProxy proxy,
                               ITorrentFileInfoReader torrentFileInfoReader,
                               IHttpClient httpClient,
                               IConfigService configService,
                               IDiskProvider diskProvider,
                               IRemotePathMappingService remotePathMappingService,
                               Logger logger,
                               ICacheManager cacheManager,
                               ISharedFolderResolver sharedFolderResolver,
                               ISerialNumberProvider serialNumberProvider)
            : base(torrentFileInfoReader, httpClient, configService, diskProvider, remotePathMappingService, logger)
        {
            _proxy = proxy;
            _sharedFolderResolver = sharedFolderResolver;
            _serialNumberProvider = serialNumberProvider;
        }

        public override string Name => "Download Station";

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            var torrents = _proxy.GetTorrents(Settings);
            var serialNumber = _serialNumberProvider.GetSerialNumber(Settings);

            var items = new List<DownloadClientItem>();

            foreach (var torrent in torrents)
            {
                var outputPath = new OsPath($"/{torrent.Additional.Detail["destination"]}");

                if (Settings.TvDirectory.IsNotNullOrWhiteSpace())
                {
                    if (!new OsPath($"/{Settings.TvDirectory}").Contains(outputPath))
                    {
                        continue;
                    }
                }
                else if (Settings.TvCategory.IsNotNullOrWhiteSpace())
                {
                    var directories = outputPath.FullPath.Split('\\', '/');
                    if (!directories.Contains(Settings.TvCategory))
                    {
                        continue;
                    }
                }

                var item = new DownloadClientItem()
                {
                    Category = Settings.TvCategory,
                    DownloadClient = Definition.Name,
                    DownloadId = CreateDownloadId(torrent.Id, serialNumber),
                    Title = torrent.Title,
                    TotalSize = torrent.Size,
                    RemainingSize = GetRemainingSize(torrent),
                    RemainingTime = GetRemainingTime(torrent),
                    Status = GetStatus(torrent),
                    Message = GetMessage(torrent),
                    IsReadOnly = !IsFinished(torrent)
                };

                if (item.Status == DownloadItemStatus.Completed || item.Status == DownloadItemStatus.Failed)
                {
                    item.OutputPath = GetOutputPath(outputPath, torrent, serialNumber);
                }

                items.Add(item);
            }

            return items;
        }

        public override DownloadClientStatus GetStatus()
        {
            try
            {
                var path = GetDownloadDirectory();

                return new DownloadClientStatus
                {
                    IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost",
                    OutputRootFolders = new List<OsPath> { _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(path)) }
                };
            }
            catch (DownloadClientException e)
            {
                _logger.Debug(e, "Failed to get config from Download Station");

                throw e;
            }
        }

        public override void RemoveItem(string downloadId, bool deleteData)
        {
            if (deleteData)
            {
                DeleteItemData(downloadId);
            }

            _proxy.RemoveTorrent(ParseDownloadId(downloadId), Settings);
            _logger.Debug("{0} removed correctly", downloadId);
        }

        protected OsPath GetOutputPath(OsPath outputPath, DownloadStationTorrent torrent, string serialNumber)
        {
            var fullPath = _sharedFolderResolver.RemapToFullPath(outputPath, Settings, serialNumber);

            var remotePath = _remotePathMappingService.RemapRemoteToLocal(Settings.Host, fullPath);

            var finalPath = remotePath + torrent.Title;

            return finalPath;
        }

        protected override string AddFromMagnetLink(RemoteEpisode remoteEpisode, string hash, string magnetLink)
        {
            var hashedSerialNumber = _serialNumberProvider.GetSerialNumber(Settings);

            _proxy.AddTorrentFromUrl(magnetLink, GetDownloadDirectory(), Settings);

            var item = _proxy.GetTorrents(Settings).SingleOrDefault(t => t.Additional.Detail["uri"] == magnetLink);

            if (item != null)
            {
                _logger.Debug("{0} added correctly", remoteEpisode);
                return CreateDownloadId(item.Id, hashedSerialNumber);
            }

            _logger.Debug("No such task {0} in Download Station", magnetLink);

            throw new DownloadClientException("Failed to add magnet task to Download Station");
        }

        protected override string AddFromTorrentFile(RemoteEpisode remoteEpisode, string hash, string filename, byte[] fileContent)
        {
            var hashedSerialNumber = _serialNumberProvider.GetSerialNumber(Settings);

            _proxy.AddTorrentFromData(fileContent, filename, GetDownloadDirectory(), Settings);

            var items = _proxy.GetTorrents(Settings).Where(t => t.Additional.Detail["uri"] == Path.GetFileNameWithoutExtension(filename));

            var item = items.SingleOrDefault();

            if (item != null)
            {
                _logger.Debug("{0} added correctly", remoteEpisode);
                return CreateDownloadId(item.Id, hashedSerialNumber);
            }

            _logger.Debug("No such task {0} in Download Station", filename);

            throw new DownloadClientException("Failed to add torrent task to Download Station");
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
            if (failures.Any()) return;
            failures.AddIfNotNull(TestGetTorrents());
        }

        protected ValidationFailure TestConnection()
        {
            try
            {
                return ValidateVersion();
            }
            catch (DownloadClientAuthenticationException ex)
            {
                _logger.Error(ex, ex.Message);
                return new NzbDroneValidationFailure("Username", "Authentication failure")
                {
                    DetailedDescription = $"Please verify your username and password. Also verify if the host running Sonarr isn't blocked from accessing {Name} by WhiteList limitations in the {Name} configuration."
                };
            }
            catch (WebException ex)
            {
                _logger.Error(ex);

                if (ex.Status == WebExceptionStatus.ConnectFailure)
                {
                    return new NzbDroneValidationFailure("Host", "Unable to connect")
                    {
                        DetailedDescription = "Please verify the hostname and port."
                    };
                }
                return new NzbDroneValidationFailure(string.Empty, "Unknown exception: " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new NzbDroneValidationFailure(string.Empty, "Unknown exception: " + ex.Message);
            }
        }

        protected ValidationFailure ValidateVersion()
        {
            var versionRange = _proxy.GetApiVersion(Settings);

            _logger.Debug("Download Station api version information: Min {0} - Max {1}", versionRange.Min(), versionRange.Max());

            if (!versionRange.Contains(2))
            {
                return new ValidationFailure(string.Empty, $"Download Station API version not supported, should be at least 2. It supports from {versionRange.Min()} to {versionRange.Max()}");
            }

            return null;
        }

        protected bool IsFinished(DownloadStationTorrent torrent)
        {
            return torrent.Status == DownloadStationTaskStatus.Finished;
        }

        protected string GetMessage(DownloadStationTorrent torrent)
        {
            if (torrent.StatusExtra != null)
            {
                if (torrent.Status == DownloadStationTaskStatus.Extracting)
                {
                    return $"Extracting: {int.Parse(torrent.StatusExtra["unzip_progress"])}%";
                }

                if (torrent.Status == DownloadStationTaskStatus.Error)
                {
                    return torrent.StatusExtra["error_detail"];
                }
            }

            return null;
        }

        protected DownloadItemStatus GetStatus(DownloadStationTorrent torrent)
        {
            switch (torrent.Status)
            {
                case DownloadStationTaskStatus.Waiting:
                    return torrent.Size == 0 || GetRemainingSize(torrent) > 0 ? DownloadItemStatus.Queued : DownloadItemStatus.Completed;
                case DownloadStationTaskStatus.Paused:
                    return DownloadItemStatus.Paused;
                case DownloadStationTaskStatus.Finished:
                case DownloadStationTaskStatus.Seeding:
                    return DownloadItemStatus.Completed;
                case DownloadStationTaskStatus.Error:
                    return DownloadItemStatus.Failed;
            }

            return DownloadItemStatus.Downloading;
        }

        protected long GetRemainingSize(DownloadStationTorrent torrent)
        {
            var downloadedString = torrent.Additional.Transfer["size_downloaded"];
            long downloadedSize;

            if (downloadedString.IsNullOrWhiteSpace() || !long.TryParse(downloadedString, out downloadedSize))
            {
                _logger.Debug("Torrent {0} has invalid size_downloaded: {1}", torrent.Title, downloadedString);
                downloadedSize = 0;
            }

            return torrent.Size - Math.Max(0, downloadedSize);
        }

        protected TimeSpan? GetRemainingTime(DownloadStationTorrent torrent)
        {
            var speedString = torrent.Additional.Transfer["speed_download"];
            long downloadSpeed;

            if (speedString.IsNullOrWhiteSpace() || !long.TryParse(speedString, out downloadSpeed))
            {
                _logger.Debug("Torrent {0} has invalid speed_download: {1}", torrent.Title, speedString);
                downloadSpeed = 0;
            }

            if (downloadSpeed <= 0)
            {
                return null;
            }

            var remainingSize = GetRemainingSize(torrent);

            return TimeSpan.FromSeconds(remainingSize / downloadSpeed);
        }

        protected ValidationFailure TestGetTorrents()
        {
            try
            {
                _proxy.GetTorrents(Settings);
                return null;
            }
            catch (Exception ex)
            {
                return new NzbDroneValidationFailure(string.Empty, "Failed to get the list of torrents: " + ex.Message);
            }
        }

        protected string ParseDownloadId(string id)
        {
            return id.Split(':')[1];
        }

        protected string CreateDownloadId(string id, string hashedSerialNumber)
        {
            return $"{hashedSerialNumber}:{id}";
        }

        protected string GetDefaultDir()
        {
            var config = _proxy.GetConfig(Settings);

            var path = config["default_destination"] as string;

            return path;
        }

        protected string GetDownloadDirectory()
        {
            if (Settings.TvDirectory.IsNotNullOrWhiteSpace())
            {
                return Settings.TvDirectory.TrimStart('/');
            }
            else if (Settings.TvCategory.IsNotNullOrWhiteSpace())
            {
                var destDir = GetDefaultDir();

                return $"{destDir.TrimEnd('/')}/{Settings.TvCategory}";
            }

            return null;
        }
    }
}
