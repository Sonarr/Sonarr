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
            : base(proxy, torrentFileInfoReader, httpClient, configService, diskProvider, remotePathMappingService, localizationService, blocklistService, logger)
        {
            _proxy = proxy
        }

        public override string Name => "Porla";

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            var plist = _proxy.ListTorrents(Settings);

            var items = new List<DownloadClientItem>();

            //should probs paginate instead of cheating with the INT64.MAXVALUE
            foreach (var torrent in plist.torrents)
            {
                // we don't need to check the category, the filter did that for us

                var outputPath = _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(torrent.save_path));
                
                var eta = TimeSpan.FromSeconds(0); //is -1 a valid eta to represent forever?
                eta = TimeSpan.FromSeconds(torrent.eta)
                //do we trust porla?
                // if (torrent.download_rate > 0 && torrent.total_done > 0) { eta = TimeSpan.FromSeconds(torrent.total / (double)torrent.download_rate); }

                var item = new DownloadClientItem
                {
                    DownloadClientInfo = DownloadClientItemClientInfo.FromDownloadClient(this, false),
                    DownloadId = torrent.info_hash.Hash.ToUpper(),
                    OutputPath = outputPath + torrent.name,
                    RemainingSize = torrent.total,
                    RemainingTime = eta,
                    Title = torrent.name,
                    TotalSize = torrent.size,
                    SeedRatio = torrent.ratio
                };

                if (string.IsNotNullOrEmpty(torrent.error))
                {
                    item.Status = DownloadItemStatus.Warning;
                    item.Message = torrent.error;
                }

                //deal with moving_storage???
                if (torrent.finished_duration > 0 || torrent.queue_position < 0)
                {
                    item.Status = DownloadItemStatus.Completed;
                }
                //else if (torrent.state == HadoukenTorrentState.QueuedForChecking)
                //{
                //    item.Status = DownloadItemStatus.Queued;
                //}
                //else if (torrent.State == HadoukenTorrentState.Paused)
                //{
                //    item.Status = DownloadItemStatus.Paused;
                //}
                else
                {
                    item.Status = DownloadItemStatus.Downloading;
                }
                // torrent.state exists, can't quite tell if it's passthrough of https://libtorrent.org/reference-Torrent_Status.html#state_t

                item.CanMoveFiles = item.CanBeRemoved = true //usure of the restrictions here

                items.Add(item);
            }

            return items;
        }

        public override void RemoveItem(DownloadClientItem item, bool deleteData)
        {
            //Kinda sucks we don't have a `RemoveItems`, porla has a batch interface for removals
            _proxy.RemoveTorrent(Settings, deleteData, [item])
            //when do we set item.Removed ?
        }

        public override DownloadClientInfo GetStatus()
        {
            //var preset = _proxy.ListPresets(Settings);
            //var sessettings = _proxy.GetSessionSettings(Settings);

            var status = new DownloadClientInfo
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost"
                RemovesCompletedDownloads = false //should be determined through Flags From 
            };

            // this value is stored in the presets values... i think?

            // destDir = new OsPath(preset.GetValueOrDefault("save_path") as string);
            //if (destDir.IsNotNullOrEmpty)
            //{
            //    status.OutputRootFolders = new List<OsPath> { _remotePathMappingService.RemapRemoteToLocal(Settings.Host, destDir) };
            //}

            return status;
        }

        protected override string AddFromMagnetLink(RemoteEpisode remoteEpisode, string hash, string magnetLink)
        {
            var torrent = _proxy.AddMagnetTorrent(Settings, magnetLink);
            return torrent.Hash.ToUpper();
        }

        protected override string AddFromTorrentFile(RemoteEpisode remoteEpisode, string hash, string filename, byte[] fileContent)
        {
            var torrent = _proxy.AddTorrentFile(Settings, fileContent);
            return torrent.Hash.ToUpper()
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
            if (failures.HasErrors())
            {
                return;
            }

            failures.AddIfNotNull(TestGetTorrents());
        }

        private ValidationFailure TestConnection()
        {
            try
            {
                var sysVers = _proxy.GetSysVersion(Settings);
                var version = new Version(sysVers.porla.version);

                if (version < new Version("0.37.0"))
                {
                    //return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("DownloadClientValidationErrorVersion",
                    //        new Dictionary<string, object> { { "clientName", Name }, { "requiredVersion", "0.37.0" }, { "reportedVersion", version } }));
                    _logger.Warn($"Porla: Unsure if your version is compatible: {version}")
                }
            }
            catch (DownloadClientAuthenticationException ex)
            {
                _logger.Error(ex, ex.Message);

                return new NzbDroneValidationFailure("Password", _localizationService.GetLocalizedString("DownloadClientValidationAuthenticationFailure"));
            }
            catch (Exception ex)
            {
                return new NzbDroneValidationFailure("Host", _localizationService.GetLocalizedString("DownloadClientValidationUnableToConnect"))
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
                _proxy.ListTorrents(Settings, 0, 1);
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
