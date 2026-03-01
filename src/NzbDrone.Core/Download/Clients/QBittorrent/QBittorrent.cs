using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Tags;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.QBittorrent
{
    public class QBittorrent : TorrentClientBase<QBittorrentSettings>
    {
        private readonly IQBittorrentProxySelector _proxySelector;
        private readonly ICached<SeedingTimeCacheEntry> _seedingTimeCache;
        private readonly ITagRepository _tagRepository;

        private class SeedingTimeCacheEntry
        {
            public DateTime LastFetched { get; set; }
            public long SeedingTime { get; set; }
        }

        public QBittorrent(IQBittorrentProxySelector proxySelector,
                           ITorrentFileInfoReader torrentFileInfoReader,
                           IHttpClient httpClient,
                           IConfigService configService,
                           IDiskProvider diskProvider,
                           IRemotePathMappingService remotePathMappingService,
                           ICacheManager cacheManager,
                           ILocalizationService localizationService,
                           IBlocklistService blocklistService,
                           ITagRepository tagRepository,
                           Logger logger)
            : base(torrentFileInfoReader, httpClient, configService, diskProvider, remotePathMappingService, localizationService, blocklistService, logger)
        {
            _proxySelector = proxySelector;

            _seedingTimeCache = cacheManager.GetCache<SeedingTimeCacheEntry>(GetType(), "seedingTime");
            _tagRepository = tagRepository;
        }

        private IQBittorrentProxy Proxy => _proxySelector.GetProxy(Settings);
        private Version ProxyApiVersion => _proxySelector.GetApiVersion(Settings);

        public override void MarkItemAsImported(DownloadClientItem downloadClientItem)
        {
            // set post-import category
            if (Settings.TvImportedCategory.IsNotNullOrWhiteSpace() &&
                Settings.TvImportedCategory != Settings.TvCategory)
            {
                try
                {
                    Proxy.SetTorrentLabel(downloadClientItem.DownloadId.ToLower(), Settings.TvImportedCategory, Settings);
                }
                catch (DownloadClientException)
                {
                    _logger.Warn("Failed to set post-import torrent label \"{0}\" for {1} in qBittorrent. Does the label exist?",
                        Settings.TvImportedCategory,
                        downloadClientItem.Title);
                }
            }
        }

        protected override string AddFromMagnetLink(RemoteEpisode remoteEpisode, string hash, string magnetLink)
        {
            if (!Proxy.GetConfig(Settings).DhtEnabled && !magnetLink.Contains("&tr="))
            {
                throw new NotSupportedException("Magnet Links without trackers not supported if DHT is disabled");
            }

            var setShareLimits = remoteEpisode.SeedConfiguration != null && (remoteEpisode.SeedConfiguration.Ratio.HasValue || remoteEpisode.SeedConfiguration.SeedTime.HasValue);
            var addHasSetShareLimits = setShareLimits && ProxyApiVersion >= new Version(2, 8, 1);
            var isRecentEpisode = remoteEpisode.IsRecentEpisode();
            var moveToTop = (isRecentEpisode && Settings.RecentTvPriority == (int)QBittorrentPriority.First) || (!isRecentEpisode && Settings.OlderTvPriority == (int)QBittorrentPriority.First);
            var forceStart = (QBittorrentState)Settings.InitialState == QBittorrentState.ForceStart;

            Proxy.AddTorrentFromUrl(magnetLink, addHasSetShareLimits && setShareLimits ? remoteEpisode.SeedConfiguration : null, Settings);

            if ((!addHasSetShareLimits && setShareLimits) || moveToTop || forceStart || (Settings.AddSeriesTags && remoteEpisode.Series.Tags.Count > 0))
            {
                if (!WaitForTorrent(hash))
                {
                    return hash;
                }

                if (!addHasSetShareLimits && setShareLimits)
                {
                    try
                    {
                        Proxy.SetTorrentSeedingConfiguration(hash.ToLower(), remoteEpisode.SeedConfiguration, Settings);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex, "Failed to set the torrent seed criteria for {0}.", hash);
                    }
                }

                if (moveToTop)
                {
                    try
                    {
                        Proxy.MoveTorrentToTopInQueue(hash.ToLower(), Settings);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex, "Failed to set the torrent priority for {0}.", hash);
                    }
                }

                if (forceStart)
                {
                    try
                    {
                        Proxy.SetForceStart(hash.ToLower(), true, Settings);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex, "Failed to set ForceStart for {0}.", hash);
                    }
                }

                if (Settings.AddSeriesTags && remoteEpisode.Series.Tags.Count > 0)
                {
                    try
                    {
                        Proxy.AddTags(hash.ToLower(), _tagRepository.GetTags(remoteEpisode.Series.Tags).Select(tag => tag.Label), Settings);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex, "Failed to add tags for {0}.", hash);
                    }
                }
            }

            return hash;
        }

        protected override string AddFromTorrentFile(RemoteEpisode remoteEpisode, string hash, string filename, byte[] fileContent)
        {
            var setShareLimits = remoteEpisode.SeedConfiguration != null && (remoteEpisode.SeedConfiguration.Ratio.HasValue || remoteEpisode.SeedConfiguration.SeedTime.HasValue);
            var addHasSetShareLimits = setShareLimits && ProxyApiVersion >= new Version(2, 8, 1);
            var isRecentEpisode = remoteEpisode.IsRecentEpisode();
            var moveToTop = (isRecentEpisode && Settings.RecentTvPriority == (int)QBittorrentPriority.First) || (!isRecentEpisode && Settings.OlderTvPriority == (int)QBittorrentPriority.First);
            var forceStart = (QBittorrentState)Settings.InitialState == QBittorrentState.ForceStart;

            Proxy.AddTorrentFromFile(filename, fileContent, addHasSetShareLimits ? remoteEpisode.SeedConfiguration : null, Settings);

            if ((!addHasSetShareLimits && setShareLimits) || moveToTop || forceStart || (Settings.AddSeriesTags && remoteEpisode.Series.Tags.Count > 0))
            {
                if (!WaitForTorrent(hash))
                {
                    return hash;
                }

                if (!addHasSetShareLimits && setShareLimits)
                {
                    try
                    {
                        Proxy.SetTorrentSeedingConfiguration(hash.ToLower(), remoteEpisode.SeedConfiguration, Settings);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex, "Failed to set the torrent seed criteria for {0}.", hash);
                    }
                }

                if (moveToTop)
                {
                    try
                    {
                        Proxy.MoveTorrentToTopInQueue(hash.ToLower(), Settings);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex, "Failed to set the torrent priority for {0}.", hash);
                    }
                }

                if (forceStart)
                {
                    try
                    {
                        Proxy.SetForceStart(hash.ToLower(), true, Settings);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex, "Failed to set ForceStart for {0}.", hash);
                    }
                }

                if (Settings.AddSeriesTags && remoteEpisode.Series.Tags.Count > 0)
                {
                    try
                    {
                        Proxy.AddTags(hash.ToLower(), _tagRepository.GetTags(remoteEpisode.Series.Tags).Select(tag => tag.Label), Settings);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex, "Failed to add tags for {0}.", hash);
                    }
                }
            }

            return hash;
        }

        protected bool WaitForTorrent(string hash)
        {
            var count = 10;

            while (count != 0)
            {
                try
                {
                    if (Proxy.IsTorrentLoaded(hash.ToLower(), Settings))
                    {
                        return true;
                    }
                }
                catch
                {
                }

                _logger.Trace("Torrent '{0}' not yet visible in qbit, waiting 100ms.", hash);
                System.Threading.Thread.Sleep(100);
                count--;
            }

            _logger.Warn("Failed to load torrent '{0}' within 500 ms, skipping additional parameters.", hash);
            return false;
        }

        public override string Name => "qBittorrent";

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            var version = Proxy.GetApiVersion(Settings);
            var config = Proxy.GetConfig(Settings);
            var torrents = Proxy.GetTorrents(Settings);

            var queueItems = new List<DownloadClientItem>();

            foreach (var torrent in torrents)
            {
                var item = new DownloadClientItem
                {
                    DownloadId = torrent.Hash.ToUpper(),
                    Category = torrent.Category.IsNotNullOrWhiteSpace() ? torrent.Category : torrent.Label,
                    Title = torrent.Name,
                    TotalSize = torrent.Size,
                    DownloadClientInfo = DownloadClientItemClientInfo.FromDownloadClient(this, Settings.TvImportedCategory.IsNotNullOrWhiteSpace()),
                    RemainingSize = (long)(torrent.Size * (1.0 - torrent.Progress)),
                    RemainingTime = GetRemainingTime(torrent),
                    SeedRatio = torrent.Ratio
                };

                // Avoid removing torrents that haven't reached the global max ratio.
                // Removal also requires the torrent to be paused, in case a higher max ratio was set on the torrent itself (which is not exposed by the api).
                item.CanMoveFiles = item.CanBeRemoved =
                    item.DownloadClientInfo.RemoveCompletedDownloads &&
                    torrent.State is "pausedUP" or "stoppedUP" &&
                    HasReachedSeedLimit(torrent, config);

                switch (torrent.State)
                {
                    case "error": // some error occurred, applies to paused torrents, warning so failed download handling isn't triggered
                        item.Status = DownloadItemStatus.Warning;
                        item.Message = _localizationService.GetLocalizedString("DownloadClientQbittorrentTorrentStateError");
                        break;

                    case "stoppedDL": // torrent is stopped and has NOT finished downloading
                    case "pausedDL": // torrent is paused and has NOT finished downloading (qBittorrent < 5)
                        item.Status = DownloadItemStatus.Paused;
                        break;

                    case "queuedDL": // queuing is enabled and torrent is queued for download
                    case "checkingDL": // same as checkingUP, but torrent has NOT finished downloading
                    case "checkingUP": // torrent has finished downloading and is being checked. Set when `recheck torrent on completion` is enabled. In the event the check fails we shouldn't treat it as completed.
                    case "checkingResumeData": // torrent is checking resume data on load
                        item.Status = DownloadItemStatus.Queued;
                        break;

                    case "pausedUP": // torrent is paused and has finished downloading (qBittorent < 5)
                    case "stoppedUP": // torrent is stopped and has finished downloading
                    case "uploading": // torrent is being seeded and data is being transferred
                    case "stalledUP": // torrent is being seeded, but no connection were made
                    case "queuedUP": // queuing is enabled and torrent is queued for upload
                    case "forcedUP": // torrent has finished downloading and is being forcibly seeded
                        item.Status = DownloadItemStatus.Completed;
                        item.RemainingTime = TimeSpan.Zero; // qBittorrent sends eta=8640000 for completed torrents
                        break;

                    case "stalledDL": // torrent is being downloaded, but no connection were made
                        item.Status = DownloadItemStatus.Warning;
                        item.Message = _localizationService.GetLocalizedString("DownloadClientQbittorrentTorrentStateStalled");
                        break;

                    case "missingFiles": // torrent is missing files
                        item.Status = DownloadItemStatus.Warning;
                        item.Message = _localizationService.GetLocalizedString("DownloadClientQbittorrentTorrentStateMissingFiles");
                        break;

                    case "metaDL": // torrent magnet is being downloaded
                    case "forcedMetaDL": // torrent metadata is being forcibly downloaded
                        if (config.DhtEnabled)
                        {
                            item.Status = DownloadItemStatus.Queued;
                            item.Message = _localizationService.GetLocalizedString("DownloadClientQbittorrentTorrentStateMetadata");
                        }
                        else
                        {
                            item.Status = DownloadItemStatus.Warning;
                            item.Message = _localizationService.GetLocalizedString("DownloadClientQbittorrentTorrentStateDhtDisabled");
                        }

                        break;

                    case "forcedDL": // torrent is being downloaded, and was forced started
                    case "moving": // torrent is being moved from a folder
                    case "downloading": // torrent is being downloaded and data is being transferred
                        item.Status = DownloadItemStatus.Downloading;
                        break;

                    default: // new status in API? default to downloading
                        item.Message = _localizationService.GetLocalizedString("DownloadClientQbittorrentTorrentStateUnknown", new Dictionary<string, object> { { "state", torrent.State } });
                        _logger.Info($"Unknown download state: {torrent.State}");
                        item.Status = DownloadItemStatus.Downloading;
                        break;
                }

                if (version >= new Version("2.6.1") && item.Status == DownloadItemStatus.Completed)
                {
                    if (torrent.ContentPath != torrent.SavePath)
                    {
                        item.OutputPath = _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(torrent.ContentPath));
                    }
                    else
                    {
                        item.Status = DownloadItemStatus.Warning;
                        item.Message = _localizationService.GetLocalizedString("DownloadClientQbittorrentTorrentStatePathError");
                    }
                }

                queueItems.Add(item);
            }

            return queueItems;
        }

        public override void RemoveItem(DownloadClientItem item, bool deleteData)
        {
            Proxy.RemoveTorrent(item.DownloadId.ToLower(), deleteData, Settings);
        }

        public override DownloadClientItem GetImportItem(DownloadClientItem item, DownloadClientItem previousImportAttempt)
        {
            // On API version >= 2.6.1 this is already set correctly
            if (!item.OutputPath.IsEmpty)
            {
                return item;
            }

            var files = Proxy.GetTorrentFiles(item.DownloadId.ToLower(), Settings);
            if (!files.Any())
            {
                _logger.Debug($"No files found for torrent {item.Title} in qBittorrent");
                return item;
            }

            var properties = Proxy.GetTorrentProperties(item.DownloadId.ToLower(), Settings);
            var savePath = new OsPath(properties.SavePath);

            var result = item.Clone();

            // get the first subdirectory - QBittorrent returns `/` path separators even on windows...
            var relativePath = new OsPath(files[0].Name);
            while (!relativePath.Directory.IsEmpty)
            {
                relativePath = relativePath.Directory;
            }

            var outputPath = savePath + relativePath.FileName;

            result.OutputPath = _remotePathMappingService.RemapRemoteToLocal(Settings.Host, outputPath);

            return result;
        }

        public override DownloadClientInfo GetStatus()
        {
            var version = Proxy.GetApiVersion(Settings);
            var config = Proxy.GetConfig(Settings);

            var destDir = new OsPath(config.SavePath);

            if (Settings.TvCategory.IsNotNullOrWhiteSpace() && version >= Version.Parse("2.0"))
            {
                if (Proxy.GetLabels(Settings).TryGetValue(Settings.TvCategory, out var label) && label.SavePath.IsNotNullOrWhiteSpace())
                {
                    var savePath = label.SavePath;

                    if (savePath.StartsWith("//"))
                    {
                        _logger.Trace("Replacing double forward slashes in path '{0}'. If this is not meant to be a Windows UNC path fix the 'Save Path' in qBittorrent's {1} category", savePath, Settings.TvCategory);
                        savePath = savePath.Replace('/', '\\');
                    }

                    var labelDir = new OsPath(savePath);

                    if (labelDir.IsRooted)
                    {
                        destDir = labelDir;
                    }
                    else
                    {
                        destDir = destDir + labelDir;
                    }
                }
            }

            return new DownloadClientInfo
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost",
                OutputRootFolders = new List<OsPath> { _remotePathMappingService.RemapRemoteToLocal(Settings.Host, destDir) },
                RemovesCompletedDownloads = RemovesCompletedDownloads(config)
            };
        }

        private bool RemovesCompletedDownloads(QBittorrentPreferences config)
        {
            var minimumRetention = 60 * 24 * 14; // 14 days in minutes
            return (config.MaxRatioEnabled || (config.MaxSeedingTimeEnabled && config.MaxSeedingTime < minimumRetention)) && (config.MaxRatioAction == QBittorrentMaxRatioAction.Remove || config.MaxRatioAction == QBittorrentMaxRatioAction.DeleteFiles);
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
            if (failures.HasErrors())
            {
                return;
            }

            failures.AddIfNotNull(TestCategory());
            failures.AddIfNotNull(TestPrioritySupport());
            failures.AddIfNotNull(TestGetTorrents());
        }

        private ValidationFailure TestConnection()
        {
            try
            {
                var version = _proxySelector.GetProxy(Settings, true).GetApiVersion(Settings);
                if (version < Version.Parse("1.5"))
                {
                    // API version 5 introduced the "save_path" property in /query/torrents
                    return new NzbDroneValidationFailure("Host", _localizationService.GetLocalizedString("DownloadClientValidationErrorVersion",
                            new Dictionary<string, object>
                            {
                                { "clientName", Name }, { "requiredVersion", "3.2.4" }, { "reportedVersion", version }
                            }));
                }
                else if (version < Version.Parse("1.6"))
                {
                    // API version 6 introduced support for labels
                    if (Settings.TvCategory.IsNotNullOrWhiteSpace())
                    {
                        return new NzbDroneValidationFailure("Category", _localizationService.GetLocalizedString("DownloadClientQbittorrentValidationCategoryUnsupported"))
                        {
                            DetailedDescription = _localizationService.GetLocalizedString("DownloadClientQbittorrentValidationCategoryUnsupportedDetail")
                        };
                    }
                }
                else if (Settings.TvCategory.IsNullOrWhiteSpace())
                {
                    // warn if labels are supported, but category is not provided
                    return new NzbDroneValidationFailure("TvCategory", _localizationService.GetLocalizedString("DownloadClientQbittorrentValidationCategoryRecommended"))
                    {
                        IsWarning = true,
                        DetailedDescription = _localizationService.GetLocalizedString("DownloadClientQbittorrentValidationCategoryRecommendedDetail")
                    };
                }

                // Complain if qBittorrent is configured to remove torrents on max ratio
                var config = Proxy.GetConfig(Settings);
                if (RemovesCompletedDownloads(config))
                {
                    return new NzbDroneValidationFailure(string.Empty, _localizationService.GetLocalizedString("DownloadClientQbittorrentValidationRemovesAtRatioLimit"))
                    {
                        DetailedDescription = _localizationService.GetLocalizedString("DownloadClientQbittorrentValidationRemovesAtRatioLimitDetail")
                    };
                }
            }
            catch (DownloadClientAuthenticationException ex)
            {
                _logger.Error(ex, ex.Message);

                return new NzbDroneValidationFailure(Settings.ApiKey.IsNotNullOrWhiteSpace() ? "ApiKey" : "Username", _localizationService.GetLocalizedString("DownloadClientValidationAuthenticationFailure"))
                {
                    DetailedDescription = _localizationService.GetLocalizedString("DownloadClientValidationAuthenticationFailureDetail", new Dictionary<string, object> { { "clientName", Name } })
                };
            }
            catch (WebException ex)
            {
                _logger.Error(ex, "Unable to connect to qBittorrent");
                if (ex.Status == WebExceptionStatus.ConnectFailure)
                {
                    return new NzbDroneValidationFailure("Host", _localizationService.GetLocalizedString("DownloadClientValidationUnableToConnect", new Dictionary<string, object> { { "clientName", Name } }))
                    {
                        DetailedDescription = _localizationService.GetLocalizedString("DownloadClientValidationUnableToConnectDetail")
                    };
                }

                return new NzbDroneValidationFailure(string.Empty, _localizationService.GetLocalizedString("DownloadClientValidationUnknownException", new Dictionary<string, object> { { "exception", ex.Message } }));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to test qBittorrent");

                return new NzbDroneValidationFailure("Host", _localizationService.GetLocalizedString("DownloadClientValidationUnableToConnect", new Dictionary<string, object> { { "clientName", Name } }))
                {
                    DetailedDescription = ex.Message
                };
            }

            return null;
        }

        private ValidationFailure TestCategory()
        {
            if (Settings.TvCategory.IsNullOrWhiteSpace() && Settings.TvImportedCategory.IsNullOrWhiteSpace())
            {
                return null;
            }

            // api v1 doesn't need to check/add categories as it's done on set
            var version = _proxySelector.GetProxy(Settings, true).GetApiVersion(Settings);
            if (version < Version.Parse("2.0"))
            {
                return null;
            }

            var labels = Proxy.GetLabels(Settings);

            if (Settings.TvCategory.IsNotNullOrWhiteSpace() && !labels.ContainsKey(Settings.TvCategory))
            {
                Proxy.AddLabel(Settings.TvCategory, Settings);
                labels = Proxy.GetLabels(Settings);

                if (!labels.ContainsKey(Settings.TvCategory))
                {
                    return new NzbDroneValidationFailure("TvCategory", _localizationService.GetLocalizedString("DownloadClientQbittorrentValidationCategoryAddFailure"))
                    {
                        DetailedDescription = _localizationService.GetLocalizedString("DownloadClientQbittorrentValidationCategoryAddFailureDetail")
                    };
                }
            }

            if (Settings.TvImportedCategory.IsNotNullOrWhiteSpace() && !labels.ContainsKey(Settings.TvImportedCategory))
            {
                Proxy.AddLabel(Settings.TvImportedCategory, Settings);
                labels = Proxy.GetLabels(Settings);

                if (!labels.ContainsKey(Settings.TvImportedCategory))
                {
                    return new NzbDroneValidationFailure("TvImportedCategory", _localizationService.GetLocalizedString("DownloadClientQbittorrentValidationCategoryAddFailure"))
                    {
                        DetailedDescription = _localizationService.GetLocalizedString("DownloadClientQbittorrentValidationCategoryAddFailureDetail")
                    };
                }
            }

            return null;
        }

        private ValidationFailure TestPrioritySupport()
        {
            var recentPriorityDefault = Settings.RecentTvPriority == (int)QBittorrentPriority.Last;
            var olderPriorityDefault = Settings.OlderTvPriority == (int)QBittorrentPriority.Last;

            if (olderPriorityDefault && recentPriorityDefault)
            {
                return null;
            }

            try
            {
                var config = Proxy.GetConfig(Settings);

                if (!config.QueueingEnabled)
                {
                    if (!recentPriorityDefault)
                    {
                        return new NzbDroneValidationFailure(nameof(Settings.RecentTvPriority), _localizationService.GetLocalizedString("DownloadClientQbittorrentValidationQueueingNotEnabled"))
                        {
                            DetailedDescription = _localizationService.GetLocalizedString("DownloadClientQbittorrentValidationQueueingNotEnabledDetail")
                        };
                    }
                    else if (!olderPriorityDefault)
                    {
                        return new NzbDroneValidationFailure(nameof(Settings.OlderTvPriority), _localizationService.GetLocalizedString("DownloadClientQbittorrentValidationQueueingNotEnabled"))
                        {
                            DetailedDescription = _localizationService.GetLocalizedString("DownloadClientQbittorrentValidationQueueingNotEnabledDetail")
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to test qBittorrent");
                return new NzbDroneValidationFailure(string.Empty, _localizationService.GetLocalizedString("DownloadClientValidationUnknownException", new Dictionary<string, object> { { "exception", ex.Message } }));
            }

            return null;
        }

        private ValidationFailure TestGetTorrents()
        {
            try
            {
                Proxy.GetTorrents(Settings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get torrents");
                return new NzbDroneValidationFailure(string.Empty, _localizationService.GetLocalizedString("DownloadClientValidationTestTorrents", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }

            return null;
        }

        protected TimeSpan? GetRemainingTime(QBittorrentTorrent torrent)
        {
            if (torrent.Eta < 0 || torrent.Eta > 365 * 24 * 3600)
            {
                return null;
            }

            // qBittorrent sends eta=8640000 if unknown such as queued
            if (torrent.Eta == 8640000)
            {
                return null;
            }

            return TimeSpan.FromSeconds((int)torrent.Eta);
        }

        protected bool HasReachedSeedLimit(QBittorrentTorrent torrent, QBittorrentPreferences config)
        {
            if (torrent.RatioLimit >= 0)
            {
                if (torrent.RatioLimit - torrent.Ratio <= 0.001f)
                {
                    return true;
                }
            }
            else if (torrent.RatioLimit == -2 && config.MaxRatioEnabled)
            {
                if (config.MaxRatio - torrent.Ratio <= 0.001f)
                {
                    return true;
                }
            }

            if (HasReachedSeedingTimeLimit(torrent, config) || HasReachedInactiveSeedingTimeLimit(torrent, config))
            {
                return true;
            }

            return false;
        }

        protected bool HasReachedSeedingTimeLimit(QBittorrentTorrent torrent, QBittorrentPreferences config)
        {
            long seedingTimeLimit;

            if (torrent.SeedingTimeLimit >= 0)
            {
                seedingTimeLimit = torrent.SeedingTimeLimit * 60;
            }
            else if (torrent.SeedingTimeLimit == -2 && config.MaxSeedingTimeEnabled)
            {
                seedingTimeLimit = config.MaxSeedingTime * 60;
            }
            else
            {
                return false;
            }

            if (torrent.SeedingTime.HasValue)
            {
                // SeedingTime can't be available here, but use it if the api starts to provide it.
                return torrent.SeedingTime.Value >= seedingTimeLimit;
            }

            var cacheKey = Settings.Host + Settings.Port + torrent.Hash;
            var cacheSeedingTime = _seedingTimeCache.Find(cacheKey);

            if (cacheSeedingTime != null)
            {
                var togo = seedingTimeLimit - cacheSeedingTime.SeedingTime;
                var elapsed = (DateTime.UtcNow - cacheSeedingTime.LastFetched).TotalSeconds;

                if (togo <= 0)
                {
                    // Already reached the limit, keep the cache alive
                    _seedingTimeCache.Set(cacheKey, cacheSeedingTime, TimeSpan.FromMinutes(5));
                    return true;
                }
                else if (togo > elapsed)
                {
                    // SeedingTime cannot have reached the required value since the last check, preserve the cache
                    _seedingTimeCache.Set(cacheKey, cacheSeedingTime, TimeSpan.FromMinutes(5));
                    return false;
                }
            }

            FetchTorrentDetails(torrent);

            cacheSeedingTime = new SeedingTimeCacheEntry
            {
                LastFetched = DateTime.UtcNow,
                SeedingTime = torrent.SeedingTime.Value
            };

            _seedingTimeCache.Set(cacheKey, cacheSeedingTime, TimeSpan.FromMinutes(5));

            if (cacheSeedingTime.SeedingTime >= seedingTimeLimit)
            {
                // Reached the limit, keep the cache alive
                return true;
            }

            return false;
        }

        protected bool HasReachedInactiveSeedingTimeLimit(QBittorrentTorrent torrent, QBittorrentPreferences config)
        {
            long inactiveSeedingTimeLimit;

            if (torrent.InactiveSeedingTimeLimit >= 0)
            {
                inactiveSeedingTimeLimit = torrent.InactiveSeedingTimeLimit * 60;
            }
            else if (torrent.InactiveSeedingTimeLimit == -2 && config.MaxInactiveSeedingTimeEnabled)
            {
                inactiveSeedingTimeLimit = config.MaxInactiveSeedingTime * 60;
            }
            else
            {
                return false;
            }

            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() - torrent.LastActivity > inactiveSeedingTimeLimit;
        }

        protected void FetchTorrentDetails(QBittorrentTorrent torrent)
        {
            var torrentProperties = Proxy.GetTorrentProperties(torrent.Hash, Settings);

            torrent.SeedingTime = torrentProperties.SeedingTime;
        }
    }
}
