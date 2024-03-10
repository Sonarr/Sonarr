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
using NzbDrone.Core.Download.Clients.Porla.Models;
using NzbDrone.Core.Localization;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Porla
{
    public class Porla : TorrentClientBase<PorlaSettings>
    {
        private readonly IPorlaProxy _proxy;

        public Porla(IPorlaProxy proxy,
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
            _proxy = proxy;
        }

        public override string Name => "Porla";

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            var plist = _proxy.ListTorrents(Settings);

            var items = new List<DownloadClientItem>();

            // should probs paginate instead of cheating with the INT64.MAXVALUE
            foreach (var torrent in plist)
            {
                // we don't need to check the category, the filter did that for us

                var outputPath = _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(torrent.SavePath));

                var eta = TimeSpan.FromSeconds(0); // is -1 a valid eta to represent forever?
                eta = TimeSpan.FromSeconds(torrent.ETA);

                // do we trust porla?
                // if (torrent.download_rate > 0 && torrent.total_done > 0) { eta = TimeSpan.FromSeconds(torrent.total / (double)torrent.download_rate); }

                var item = new DownloadClientItem
                {
                    DownloadClientInfo = DownloadClientItemClientInfo.FromDownloadClient(this, false),
                    DownloadId = torrent.InfoHash.Hash,
                    OutputPath = outputPath + torrent.Name,
                    RemainingSize = torrent.Total,
                    RemainingTime = eta,
                    Title = torrent.Name,
                    TotalSize = torrent.Size,
                    SeedRatio = torrent.Ratio
                };

                if (!string.IsNullOrEmpty(torrent.Error))
                {
                    item.Status = DownloadItemStatus.Warning;
                    item.Message = torrent.Error;
                }

                _logger.Debug($"Torrent {torrent.InfoHash.Hash} state is {torrent.State}");

                // deal with moving_storage???
                if (torrent.FinishedDuration > 0)
                {
                    item.Status = DownloadItemStatus.Completed;
                }

                // else if (torrent.state == HadoukenTorrentState.QueuedForChecking)
                // {
                //    item.Status = DownloadItemStatus.Queued;
                // }
                // else if (torrent.State == HadoukenTorrentState.Paused)
                // {
                //    item.Status = DownloadItemStatus.Paused;
                // }
                else
                {
                    item.Status = DownloadItemStatus.Downloading;
                }

                // torrent.state exists, can't quite tell if it's passthrough of https://libtorrent.org/reference-Torrent_Status.html#state_t

                item.CanMoveFiles = item.CanBeRemoved = true; // usure of the restrictions here

                items.Add(item);
            }

            return items;
        }

        public override void RemoveItem(DownloadClientItem item, bool deleteData)
        {
            // Kinda sucks we don't have a `RemoveItems`, porla has a batch interface for removals
            PorlaTorrent[] singleItem = { new PorlaTorrent(item.DownloadId.ToLower(), "") };
            _proxy.RemoveTorrent(Settings, deleteData, singleItem);

            // when do we set item.Removed ?
        }

        public override DownloadClientInfo GetStatus()
        {
            var presetEffectiveSettings = _proxy.ListPresets(Settings).GetEffective(Settings.Preset.IsNullOrWhiteSpace() ? "default" : Settings.Preset);

            // var sessionSettings = _proxy.GetSessionSettings(Settings);

            var status = new DownloadClientInfo
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost",
                RemovesCompletedDownloads = false, // TODO: I don't think porla has config for this
            };

            var savePath = (presetEffectiveSettings?.SavePath.IsNullOrWhiteSpace() ?? true ? Settings.TvDirectory : presetEffectiveSettings?.SavePath) ?? "";
            var destDir = new OsPath(savePath);
            if (!destDir.IsEmpty)
            {
               status.OutputRootFolders = new List<OsPath> { _remotePathMappingService.RemapRemoteToLocal(Settings.Host, destDir) };
            }

            return status;
        }

        private static IList<string> ConvertRemoteEpisodeToTags(RemoteEpisode remoteEpisode)
        {
            var tags = new List<string>
            {
                $"starr.series={remoteEpisode.Series.CleanTitle}",
                $"starr.season={remoteEpisode.MappedSeasonNumber}",
                $"starr.tvdbid={remoteEpisode.Series.TvdbId}"
            };
            return tags;
        }

        protected override string AddFromMagnetLink(RemoteEpisode remoteEpisode, string hash, string magnetLink)
        {
            var tags = Settings.SeriesTag ? ConvertRemoteEpisodeToTags(remoteEpisode) : null;
            var torrent = _proxy.AddMagnetTorrent(Settings, magnetLink, tags);
            return torrent.InfoHash.Hash;
        }

        protected override string AddFromTorrentFile(RemoteEpisode remoteEpisode, string hash, string filename, byte[] fileContent)
        {
            var tags = Settings.SeriesTag ? ConvertRemoteEpisodeToTags(remoteEpisode) : null;
            var torrent = _proxy.AddTorrentFile(Settings, fileContent, tags);
            return torrent.InfoHash.Hash;
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestVersion());
            if (failures.HasErrors())
            {
                return;
            }

            failures.AddIfNotNull(TestGetTorrents());
        }

        /// <summary> Test the connection by calling the `sys.version` </summary>
        private ValidationFailure TestVersion()
        {
            // TODO : Consider dropping the future version (painful to keep up with)
            try
            {
                // Version Compatability check.
                var sysVers = _proxy.GetSysVersion(Settings);
                var badVersions = new List<string> { };         // List of Broken Versions
                var goodVersion = new Version("0.37.0");        // The main version we want to see
                var firstGoodVersion = new Version("0.37.0");   // The first (cronological) version that we are sure works (usually the goodVersion)
                var lastGoodVersion = new Version("0.37.0");    // The last (cronological) version that we are sure works (usually the goodVersion)
                var actualVersion = new Version(sysVers.Porla.Version);

                if (badVersions.Any(s => new Version(s) == actualVersion))
                {
                    _logger.Error($"Porla: Your Porla version isn't compatible with Sonarr!: {actualVersion}");
                    return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("DownloadClientValidationErrorVersion",
                            new Dictionary<string, object> { { "clientName", Name }, { "requiredVersion", goodVersion.ToString() }, { "reportedVersion", actualVersion } }));
                }

                if (actualVersion < firstGoodVersion)
                {
                    _logger.Warn($"Porla: Your version might not be forwards compatible: {actualVersion}");
                }

                if (actualVersion > lastGoodVersion)
                {
                    _logger.Warn($"Porla: Your version might not be backwards compatible: {actualVersion}");
                }

                if (actualVersion < new Version("1.0.0"))
                {
                    _logger.Warn($"Porla: Porla is in active development, expect weirdness with it and this client");
                }
            }
            catch (DownloadClientAuthenticationException ex)
            {
                _logger.Error(ex, ex.Message);

                return new NzbDroneValidationFailure("Password", _localizationService.GetLocalizedString("DownloadClientValidationAuthenticationFailure"));
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

            return null;
        }

        private ValidationFailure TestGetTorrents()
        {
            try
            {
                _ = _proxy.ListTorrents(Settings, 0, 1);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return new NzbDroneValidationFailure(string.Empty, _localizationService.GetLocalizedString("DownloadClientValidationTestTorrents", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }

            return null;
        }
    }
}
