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

            // should probs paginate instead of cheating
            foreach (var torrent in plist)
            {
                var outputPath = _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(torrent.SavePath));

                var item = new DownloadClientItem
                {
                    DownloadClientInfo = DownloadClientItemClientInfo.FromDownloadClient(this, false),
                    DownloadId = torrent.InfoHash.Hash,
                    OutputPath = outputPath + torrent.Name,
                    RemainingSize = torrent.Total,
                    RemainingTime = torrent.ETA < 1 ? (TimeSpan?)null : TimeSpan.FromSeconds((double)torrent.ETA), // LibTorent uses `-1` to denote "infinite" time i.e. I am stuck, or I am done. FromSeconds will convert `-1` to 1, so we need to do this.
                    Title = torrent.Name,
                    TotalSize = torrent.Size,
                    SeedRatio = torrent.Ratio
                };

                // deal with moving_storage=true ?

                if (!string.IsNullOrEmpty(torrent.Error))
                {
                    item.Status = DownloadItemStatus.Warning;
                    item.Message = torrent.Error;
                } // TODO: check paused or finished first?
                else if (torrent.FinishedDuration > 0)
                {
                    item.Status = DownloadItemStatus.Completed;
                    if (torrent.ETA < 1)
                    {
                        // Sonarr wants to see a TimeSpan.Zero when it is done ( -1 -> 0 )
                        item.RemainingTime = TimeSpan.Zero;
                    }
                }
                else if (torrent.Flags.Contains("paused"))
                {
                   item.Status = DownloadItemStatus.Paused;
                } /*  I don't know what the torent looks like if it is "Queued"
                else if (???)
                {
                   item.Status = DownloadItemStatus.Queued;
                } */
                else
                {
                    item.Status = DownloadItemStatus.Downloading;
                }

                item.CanMoveFiles = item.CanBeRemoved = true; // usure of what restricts this on porla. Currently these is always true

                items.Add(item);
            }

            if (items.Count < 1)
            {
                _logger.Debug("No Items Returned");
            }

            return items;
        }

        public override void RemoveItem(DownloadClientItem item, bool deleteData)
        {
            // Kinda sucks we don't have a `RemoveItems`, porla has a batch interface for removals
            PorlaTorrent[] singleItem = { new PorlaTorrent(item.DownloadId, "") };
            _proxy.RemoveTorrent(Settings, deleteData, singleItem);
        }

        public override DownloadClientInfo GetStatus()
        {
            var presetEffectiveSettings = _proxy.ListPresets(Settings).GetEffective(Settings.Preset.IsNullOrWhiteSpace() ? "default" : Settings.Preset);

            // var sessionSettings = _proxy.GetSessionSettings(Settings);

            var status = new DownloadClientInfo
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost",
                RemovesCompletedDownloads = false, // TODO: I don't think porla has config for this, it feels like it should
            };

            var savePath = ((presetEffectiveSettings?.SavePath.IsNullOrWhiteSpace() ?? true) ? Settings.TvDirectory : presetEffectiveSettings?.SavePath) ?? "";
            var destDir = new OsPath(savePath);

            if (!destDir.IsEmpty)
            {
               status.OutputRootFolders = new List<OsPath> { _remotePathMappingService.RemapRemoteToLocal(Settings.Host, destDir) };
            }

            return status;
        }

        /// <summary> Converts a RemoteEpisode into a list of <em>starr</em> tags </summary>
        /// <see cref="RemoteEpisode"/>
        private static IList<string> ConvertRemoteEpisodeToTags(RemoteEpisode remoteEpisode)
        {
            var tags = new List<string>
            {
                $"starr.series={remoteEpisode.Series.CleanTitle}",
                $"starr.season={remoteEpisode.MappedSeasonNumber}",
                $"starr.tvdbid={remoteEpisode.Series.TvdbId}",
                $"starr.imdbid={remoteEpisode.Series.ImdbId}",
                $"starr.tvmazeid={remoteEpisode.Series.TvMazeId}",
                $"starr.year={remoteEpisode.Series.Year}"
            };
            return tags;
        }

        // NOTE: If the torrent already exists in the client, it'll fail with error code -3
        // {
        //   "code": -3,
        //   "data": null,
        //   "message": "Torrent already in session 'default'"
        // }
        // IDK If I should deal with that...

        protected override string AddFromMagnetLink(RemoteEpisode remoteEpisode, string hash, string magnetLink)
        {
            PorlaTorrent torrent;

            if (Settings.SeriesTag)
            {
                var tags = ConvertRemoteEpisodeToTags(remoteEpisode);
                torrent = _proxy.AddMagnetTorrent(Settings, magnetLink, tags);
            }
            else
            {
                torrent = _proxy.AddMagnetTorrent(Settings, magnetLink);
            }

            return torrent.InfoHash.Hash ?? "";
        }

        protected override string AddFromTorrentFile(RemoteEpisode remoteEpisode, string hash, string filename, byte[] fileContent)
        {
            PorlaTorrent torrent;

            if (Settings.SeriesTag)
            {
                var tags = ConvertRemoteEpisodeToTags(remoteEpisode);
                torrent = _proxy.AddTorrentFile(Settings, fileContent, tags);
            }
            else
            {
                torrent = _proxy.AddTorrentFile(Settings, fileContent);
            }

            return torrent.InfoHash.Hash ?? "";
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
                    _logger.Error($"Your Porla version isn't compatible with Sonarr!: {actualVersion}");
                    return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("DownloadClientValidationErrorVersion",
                            new Dictionary<string, object> { { "clientName", Name }, { "requiredVersion", goodVersion.ToString() }, { "reportedVersion", actualVersion } }));
                }

                if (actualVersion < firstGoodVersion)
                {
                    _logger.Warn($"Your version might not be forwards compatible: {actualVersion}");
                }

                if (actualVersion > lastGoodVersion)
                {
                    _logger.Warn($"Your version might not be backwards compatible: {actualVersion}");
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
