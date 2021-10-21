using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using MonoTorrent;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;

using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers.Tribler;
using NzbDrone.Core.Localization;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Tribler
{
    public class TriblerDownloadClient : TorrentClientBase<TriblerDownloadSettings>
    {
        private readonly ITriblerDownloadClientProxy _proxy;

        public TriblerDownloadClient(
            ITriblerDownloadClientProxy triblerDownloadClientProxy,
            ITorrentFileInfoReader torrentFileInfoReader,
            IHttpClient httpClient,
            IConfigService configService,
            IDiskProvider diskProvider,
            IRemotePathMappingService remotePathMappingService,
            ILocalizationService localizationService,
            IBlocklistService blocklistService,
            Logger logger)
            : base(torrentFileInfoReader, httpClient, configService, diskProvider, remotePathMappingService, localizationService, blocklistService, logger)
        {
            _proxy = triblerDownloadClientProxy;
        }

        public override string Name => "Tribler";

        public override bool PreferTorrentFile => false;

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            var configAsync = _proxy.GetConfig(Settings);

            var items = new List<DownloadClientItem>();

            var downloads = _proxy.GetDownloads(Settings);

            foreach (var download in downloads)
            {
                // If totalsize == 0 the torrent is a magnet downloading metadata
                if (download.Size == null || download.Size == 0)
                {
                    continue;
                }

                // skip channel downloads
                if (download.ChannelDownload == true)
                {
                    continue;
                }

                var item = new DownloadClientItem
                {
                    DownloadId = InfoHash.FromHex(download.Infohash).ToHex(),
                    Title = download.Name,

                    DownloadClientInfo = DownloadClientItemClientInfo.FromDownloadClient(this, false) // TODO: WHAT IS POST-IMPORT
                };

                // some concurrency could make this faster.
                var files = _proxy.GetDownloadFiles(Settings, download);

                item.OutputPath = new OsPath(download.Destination);

                if (files.Count == 1)
                {
                    item.OutputPath += files.First().Name;
                }
                else
                {
                    item.OutputPath += item.Title;
                }

                item.TotalSize = (long)download.Size;
                item.RemainingSize = (long)(download.Size * (1 - download.Progress)); // TODO: i expect progress to be between 0 and 1
                item.SeedRatio = download.Ratio;

                if (download.Eta.HasValue)
                {
                    if (download.Eta.Value >= TimeSpan.FromDays(365).TotalSeconds)
                    {
                        item.RemainingTime = TimeSpan.FromDays(365);
                    }
                    else if (download.Eta.Value < 0)
                    {
                        item.RemainingTime = TimeSpan.FromSeconds(0);
                    }
                    else
                    {
                        item.RemainingTime = TimeSpan.FromSeconds(download.Eta.Value);
                    }
                }

                // TODO: the item's message should not be equal to Error
                item.Message = download.Error;

                // tribler always saves files unencrypted to disk.
                item.IsEncrypted = false;

                // state handling

                // TODO: impossible states?
                // Failed = 4,
                // Warning = 5

                // Queued = 0,
                // Completed = 3,
                // Downloading = 2,

                // Paused is guesstimated
                // Paused = 1,

                switch (download.Status)
                {
                    case DownloadStatus.HASHCHECKING:
                    case DownloadStatus.WAITING4HASHCHECK:
                    case DownloadStatus.CIRCUITS:
                    case DownloadStatus.EXIT_NODES:
                    case DownloadStatus.DOWNLOADING:
                        item.Status = DownloadItemStatus.Downloading;
                        break;
                    case DownloadStatus.METADATA:
                    case DownloadStatus.ALLOCATING_DISKSPACE:
                        item.Status = DownloadItemStatus.Queued;
                        break;
                    case DownloadStatus.SEEDING:
                    case DownloadStatus.STOPPED:
                        item.Status = DownloadItemStatus.Completed;
                        break;
                    case DownloadStatus.STOPPED_ON_ERROR:
                        item.Status = DownloadItemStatus.Failed;
                        break;
                    default: // new status in API? default to downloading
                        item.Message = "Unknown download state: " + download.Status;
                        _logger.Info(item.Message);
                        item.Status = DownloadItemStatus.Downloading;
                        break;
                }

                // override status' if completed but progress is not finished
                if (download.Status == DownloadStatus.STOPPED && download.Progress < 1)
                {
                    item.Status = DownloadItemStatus.Paused;
                }

                // override status if error is set
                if (download.Error != null && download.Error.Length > 0)
                {
                    item.Status = DownloadItemStatus.Warning; // maybe this should be an error?
                }

                // done (finished seeding & stopped, guessed)
                item.CanBeRemoved = HasReachedSeedLimit(download, configAsync);

                // seeding or done, or stopped
                item.CanMoveFiles = download.Progress == 1.0;

                items.Add(item);
            }

            return items;
        }

        public override void RemoveItem(DownloadClientItem item, bool deleteData)
        {
            _proxy.RemoveDownload(Settings, item, deleteData);
        }

        public override DownloadClientInfo GetStatus()
        {
            var config = _proxy.GetConfig(Settings);
            var destDir = config.Settings.Download_defaults.Saveas;

            if (Settings.TvCategory.IsNotNullOrWhiteSpace())
            {
                destDir = string.Format("{0}/.{1}", destDir, Settings.TvCategory);
            }

            return new DownloadClientInfo
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost",
                OutputRootFolders = new List<OsPath> { _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(destDir)) }
            };
        }

        /**
         * this basically checks if torrent is stopped because of seeding has finished
         */
        protected static bool HasReachedSeedLimit(Download torrent, GetTriblerSettingsResponse config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (torrent == null)
            {
                throw new ArgumentNullException(nameof(torrent));
            }

            // if download is still running then it's not finished.
            if (torrent.Status != DownloadStatus.STOPPED)
            {
                return false;
            }

            switch (config.Settings.Download_defaults.Seeding_mode)
            {
                // if in ratio mode, wait for ratio to become larger than expeced. Tribler's DownloadStatus will switch from SEEDING to STOPPED
                case Download_defaultsSeeding_mode.Ratio:

                    return torrent.Ratio.HasValue
                        && torrent.Ratio >= config.Settings.Download_defaults.Seeding_ratio;

                case Download_defaultsSeeding_mode.Time:
                    var downloadStarted = DateTimeOffset.FromUnixTimeSeconds(torrent.Time_added.Value);
                    var maxSeedingTime = TimeSpan.FromSeconds(config.Settings.Download_defaults.Seeding_time ?? 0);

                    return torrent.Time_added.HasValue
                        && downloadStarted.Add(maxSeedingTime) < DateTimeOffset.Now;

                case Download_defaultsSeeding_mode.Never:
                    return true;

                case Download_defaultsSeeding_mode.Forever:
                default:
                    return false;
            }
        }

        protected override string AddFromMagnetLink(RemoteEpisode remoteEpisode, string hash, string magnetLink)
        {
            var addDownloadRequestObject = new AddDownloadRequest
            {
                Destination = GetDownloadDirectory(),
                Uri = magnetLink,
                Safe_seeding = Settings.SafeSeeding,
                Anon_hops = Settings.AnonymityLevel
            };

            return _proxy.AddFromMagnetLink(Settings, addDownloadRequestObject);
        }

        protected override string AddFromTorrentFile(RemoteEpisode remoteEpisode, string hash, string filename, byte[] fileContent)
        {
            // tribler api currently do not support recieving a torrent file direcly.
            throw new NotSupportedException("Tribler download client only support magnet links currently");
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
            if (failures.HasErrors())
            {
                return;
            }

            // failures.AddIfNotNull(TestGetTorrents());
        }

        protected string GetDownloadDirectory()
        {
            if (Settings.TvDirectory.IsNotNullOrWhiteSpace())
            {
                return Settings.TvDirectory;
            }

            if (!Settings.TvCategory.IsNotNullOrWhiteSpace())
            {
                return null;
            }

            var config = _proxy.GetConfig(Settings);
            var destDir = config.Settings.Download_defaults.Saveas;

            return $"{destDir.TrimEnd('/')}/{Settings.TvCategory}";
        }

        protected ValidationFailure TestConnection()
        {
            try
            {
                var downloads = GetItems();
                return null;
            }
            catch (DownloadClientAuthenticationException ex)
            {
                _logger.Error(ex, ex.Message);

                return new NzbDroneValidationFailure("ApiKey", "Authentication failure")
                {
                    DetailedDescription = string.Format("Please verify your ApiKey is correct. Also verify if the host running Sonarr isn't blocked from accessing {0} by WhiteList limitations in the {0} configuration.", Name)
                };
            }
            catch (DownloadClientUnavailableException ex)
            {
                _logger.Error(ex, ex.Message);

                return new NzbDroneValidationFailure("Url", "Unable to connect to Tribler")
                {
                    DetailedDescription = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to test");

                return new NzbDroneValidationFailure(string.Empty, "Unknown exception: " + ex.Message);
            }
        }
    }
}
