using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.Clients.rTorrent;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Localization;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.RTorrent
{
    public class RTorrent : TorrentClientBase<RTorrentSettings>
    {
        private readonly IRTorrentProxy _proxy;
        private readonly IRTorrentDirectoryValidator _rTorrentDirectoryValidator;
        private readonly IDownloadSeedConfigProvider _downloadSeedConfigProvider;
        private readonly string _imported_view = string.Concat(BuildInfo.AppName.ToLower(), "_imported");

        public RTorrent(IRTorrentProxy proxy,
                        ITorrentFileInfoReader torrentFileInfoReader,
                        IHttpClient httpClient,
                        IConfigService configService,
                        IDiskProvider diskProvider,
                        IRemotePathMappingService remotePathMappingService,
                        IDownloadSeedConfigProvider downloadSeedConfigProvider,
                        IRTorrentDirectoryValidator rTorrentDirectoryValidator,
                        ILocalizationService localizationService,
                        IBlocklistService blocklistService,
                        Logger logger)
            : base(torrentFileInfoReader, httpClient, configService, diskProvider, remotePathMappingService, localizationService, blocklistService, logger)
        {
            _proxy = proxy;
            _rTorrentDirectoryValidator = rTorrentDirectoryValidator;
            _downloadSeedConfigProvider = downloadSeedConfigProvider;
        }

        public override void MarkItemAsImported(DownloadClientItem downloadClientItem)
        {
            // Set post-import label
            if (Settings.TvImportedCategory.IsNotNullOrWhiteSpace() &&
                Settings.TvImportedCategory != Settings.TvCategory)
            {
                try
                {
                    _proxy.SetTorrentLabel(downloadClientItem.DownloadId.ToLower(), Settings.TvImportedCategory, Settings);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex,
                        "Failed to set torrent post-import label \"{0}\" for {1} in rTorrent. Does the label exist?",
                        Settings.TvImportedCategory,
                        downloadClientItem.Title);
                }
            }

            // Set post-import view
            try
            {
                _proxy.PushTorrentUniqueView(downloadClientItem.DownloadId.ToLower(), _imported_view, Settings);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex,
                    "Failed to set torrent post-import view \"{0}\" for {1} in rTorrent.",
                    _imported_view,
                    downloadClientItem.Title);
            }
        }

        protected override string AddFromMagnetLink(RemoteEpisode remoteEpisode, string hash, string magnetLink)
        {
            var priority = (RTorrentPriority)(remoteEpisode.IsRecentEpisode() ? Settings.RecentTvPriority : Settings.OlderTvPriority);

            _proxy.AddTorrentFromUrl(magnetLink, Settings.TvCategory, priority, Settings.TvDirectory, Settings);

            var tries = 10;
            var retryDelay = 500;

            // Wait a bit for the magnet to be resolved.
            if (!WaitForTorrent(hash, tries, retryDelay))
            {
                _logger.Warn("rTorrent could not resolve magnet within {0} seconds, download may remain stuck: {1}.", tries * retryDelay / 1000, magnetLink);

                return hash;
            }

            return hash;
        }

        protected override string AddFromTorrentFile(RemoteEpisode remoteEpisode, string hash, string filename, byte[] fileContent)
        {
            var priority = (RTorrentPriority)(remoteEpisode.IsRecentEpisode() ? Settings.RecentTvPriority : Settings.OlderTvPriority);

            _proxy.AddTorrentFromFile(filename, fileContent, Settings.TvCategory, priority, Settings.TvDirectory, Settings);

            var tries = 10;
            var retryDelay = 500;
            if (!WaitForTorrent(hash, tries, retryDelay))
            {
                _logger.Debug("rTorrent didn't add the torrent within {0} seconds: {1}.", tries * retryDelay / 1000, filename);

                throw new ReleaseDownloadException(remoteEpisode.Release, "Downloading torrent failed");
            }

            return hash;
        }

        public override string Name => "rTorrent";

        public override ProviderMessage Message => new ProviderMessage(_localizationService.GetLocalizedString("DownloadClientRTorrentProviderMessage", new Dictionary<string, object> { { "importedView", _imported_view } }), ProviderMessageType.Info);

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            var torrents = _proxy.GetTorrents(Settings);

            _logger.Debug("Retrieved metadata of {0} torrents in client", torrents.Count);

            var items = new List<DownloadClientItem>();
            foreach (var torrent in torrents)
            {
                // Don't concern ourselves with categories other than specified
                if (Settings.TvCategory.IsNotNullOrWhiteSpace() && torrent.Category != Settings.TvCategory)
                {
                    continue;
                }

                // Ignore torrents with an empty path
                if (torrent.Path.IsNullOrWhiteSpace())
                {
                    _logger.Warn("Torrent '{0}' has an empty download path and will not be processed. Adjust this to an absolute path in rTorrent", torrent.Name);
                    continue;
                }

                if (torrent.Path.StartsWith("."))
                {
                    _logger.Warn("Torrent '{0}' has a download path starting with '.' and will not be processed. Adjust this to an absolute path in rTorrent", torrent.Name);
                    continue;
                }

                var item = new DownloadClientItem();
                item.DownloadClientInfo = DownloadClientItemClientInfo.FromDownloadClient(this, Settings.TvImportedCategory.IsNotNullOrWhiteSpace());
                item.Title = torrent.Name;
                item.DownloadId = torrent.Hash;
                item.OutputPath = _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(torrent.Path));
                item.TotalSize = torrent.TotalSize;
                item.RemainingSize = torrent.RemainingSize;
                item.Category = torrent.Category;
                item.SeedRatio = torrent.Ratio;

                if (torrent.DownRate > 0)
                {
                    var secondsLeft = torrent.RemainingSize / torrent.DownRate;
                    item.RemainingTime = TimeSpan.FromSeconds(secondsLeft);
                }
                else
                {
                    item.RemainingTime = TimeSpan.Zero;
                }

                if (torrent.IsFinished)
                {
                    item.Status = DownloadItemStatus.Completed;
                }
                else if (torrent.IsActive)
                {
                    item.Status = DownloadItemStatus.Downloading;
                }
                else if (!torrent.IsActive)
                {
                    item.Status = DownloadItemStatus.Paused;
                }

                // Grab cached seedConfig
                var seedConfig = _downloadSeedConfigProvider.GetSeedConfiguration(torrent.Hash);

                if (torrent.IsFinished && seedConfig != null)
                {
                    var canRemove = false;

                    if (torrent.Ratio / 1000.0 >= seedConfig.Ratio)
                    {
                        _logger.Trace($"{item} has met seed ratio goal of {seedConfig.Ratio}");
                        canRemove = true;
                    }
                    else if (DateTimeOffset.Now - DateTimeOffset.FromUnixTimeSeconds(torrent.FinishedTime) >= seedConfig.SeedTime)
                    {
                        _logger.Trace($"{item} has met seed time goal of {seedConfig.SeedTime} minutes");
                        canRemove = true;
                    }
                    else
                    {
                        _logger.Trace($"{item} seeding goals have not yet been reached");
                    }

                    // Check if torrent is finished and if it exceeds cached seedConfig
                    item.CanMoveFiles = item.CanBeRemoved = canRemove;
                }

                items.Add(item);
            }

            return items;
        }

        public override void RemoveItem(DownloadClientItem item, bool deleteData)
        {
            if (deleteData)
            {
                DeleteItemData(item);
            }

            _proxy.RemoveTorrent(item.DownloadId, Settings);
        }

        public override DownloadClientInfo GetStatus()
        {
            // XXX: This function's correctness has not been considered

            var status = new DownloadClientInfo
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost"
            };

            return status;
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
            if (failures.HasErrors())
            {
                return;
            }

            failures.AddIfNotNull(TestGetTorrents());
            failures.AddIfNotNull(TestDirectory());
        }

        private ValidationFailure TestConnection()
        {
            try
            {
                var version = _proxy.GetVersion(Settings);

                if (new Version(version) < new Version("0.9.0"))
                {
                    return new ValidationFailure(string.Empty,
                        _localizationService.GetLocalizedString("DownloadClientValidationErrorVersion",
                            new Dictionary<string, object>
                            {
                                { "clientName", Name }, { "requiredVersion", "0.9.0" }, { "reportedVersion", version }
                            }));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to test rTorrent");

                return new NzbDroneValidationFailure("Host", _localizationService.GetLocalizedString("DownloadClientValidationUnableToConnect", new Dictionary<string, object> { { "clientName", Name } }))
                       {
                           DetailedDescription = ex.Message
                       };
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
                _logger.Error(ex, "Failed to get torrents");
                return new NzbDroneValidationFailure(string.Empty, _localizationService.GetLocalizedString("DownloadClientValidationTestTorrents", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }

            return null;
        }

        private ValidationFailure TestDirectory()
        {
            var result = _rTorrentDirectoryValidator.Validate(Settings);

            if (result.IsValid)
            {
                return null;
            }

            return result.Errors.First();
        }

        private bool WaitForTorrent(string hash, int tries, int retryDelay)
        {
            for (var i = 0; i < tries; i++)
            {
                if (_proxy.HasHashTorrent(hash, Settings))
                {
                    return true;
                }

                Thread.Sleep(retryDelay);
            }

            _logger.Debug("Could not find hash {0} in {1} tries at {2} ms intervals.", hash, tries, retryDelay);

            return false;
        }
    }
}
