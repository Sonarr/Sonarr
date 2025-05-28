using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
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

                var item = new DownloadClientItem
                {
                    DownloadId = download.Infohash,
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
                item.SeedRatio = download.AllTimeRatio;

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

                item.Message = download.Error;

                // tribler always saves files unencrypted to disk.
                item.IsEncrypted = false;

                switch (download.Status)
                {
                    case DownloadStatus.Hashchecking:
                    case DownloadStatus.Waiting4HashCheck:
                    case DownloadStatus.Circuits:
                    case DownloadStatus.Exitnodes:
                    case DownloadStatus.Downloading:
                        item.Status = DownloadItemStatus.Downloading;
                        break;
                    case DownloadStatus.Metadata:
                    case DownloadStatus.AllocatingDiskspace:
                        item.Status = DownloadItemStatus.Queued;
                        break;
                    case DownloadStatus.Seeding:
                    case DownloadStatus.Stopped:
                        item.Status = DownloadItemStatus.Completed;
                        break;
                    case DownloadStatus.StoppedOnError:
                        item.Status = DownloadItemStatus.Failed;
                        break;
                    default: // new status in API? default to downloading
                        item.Message = "Unknown download state: " + download.Status;
                        _logger.Info(item.Message);
                        item.Status = DownloadItemStatus.Downloading;
                        break;
                }

                // Override status if completed, but not finished downloading
                if (download.Status == DownloadStatus.Stopped && download.Progress < 1)
                {
                    item.Status = DownloadItemStatus.Paused;
                }

                if (download.Error != null && download.Error.Length > 0)
                {
                    item.Status = DownloadItemStatus.Warning;
                    item.Message = download.Error;
                }

                item.CanBeRemoved = item.CanMoveFiles = HasReachedSeedLimit(download, configAsync);

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
            var destDir = config.Settings.LibTorrent.DownloadDefaults.SaveAS;

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

        protected static bool HasReachedSeedLimit(Download torrent, TriblerSettingsResponse config)
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
            if (torrent.Status != DownloadStatus.Stopped)
            {
                return false;
            }

            switch (config.Settings.LibTorrent.DownloadDefaults.SeedingMode)
            {
                case DownloadDefaultsSeedingMode.Ratio:

                    return torrent.AllTimeRatio.HasValue
                        && torrent.AllTimeRatio >= config.Settings.LibTorrent.DownloadDefaults.SeedingRatio;

                case DownloadDefaultsSeedingMode.Time:
                    var downloadStarted = DateTimeOffset.FromUnixTimeSeconds(torrent.TimeAdded.Value);
                    var maxSeedingTime = TimeSpan.FromSeconds(config.Settings.LibTorrent.DownloadDefaults.SeedingTime ?? 0);

                    return torrent.TimeAdded.HasValue
                        && downloadStarted.Add(maxSeedingTime) < DateTimeOffset.Now;

                case DownloadDefaultsSeedingMode.Never:
                    return true;

                case DownloadDefaultsSeedingMode.Forever:
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
                SafeSeeding = Settings.SafeSeeding,
                AnonymityHops = Settings.AnonymityLevel
            };

            return _proxy.AddFromMagnetLink(Settings, addDownloadRequestObject);
        }

        protected override string AddFromTorrentFile(RemoteEpisode remoteEpisode, string hash, string filename, byte[] fileContent)
        {
            // TODO: Tribler 8.x does support adding from a torrent file, but it's not a simple put command.
            throw new NotSupportedException("Tribler does not support torrent files, only magnet links");
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());

            if (failures.HasErrors())
            {
                return;
            }
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
            var destDir = config.Settings.LibTorrent.DownloadDefaults.SaveAS;

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

                return new ValidationFailure("ApiKey", _localizationService.GetLocalizedString("DownloadClientValidationApiKeyIncorrect"));
            }
            catch (DownloadClientUnavailableException ex)
            {
                _logger.Error(ex, ex.Message);

                return new NzbDroneValidationFailure("Host", _localizationService.GetLocalizedString("DownloadClientValidationUnableToConnect", new Dictionary<string, object> { { "clientName", Name } }))
                {
                    DetailedDescription = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to test");

                return new NzbDroneValidationFailure(string.Empty, _localizationService.GetLocalizedString("DownloadClientValidationUnknownException", new Dictionary<string, object> { { "exception", ex.Message } }));
            }
        }
    }
}
