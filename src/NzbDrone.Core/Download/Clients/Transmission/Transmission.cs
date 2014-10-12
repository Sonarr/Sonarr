using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Configuration;
using NLog;
using FluentValidation.Results;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Validation;
using System.Net;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;

namespace NzbDrone.Core.Download.Clients.Transmission
{
    public class Transmission : TorrentClientBase<TransmissionSettings>
    {
        private readonly ITransmissionProxy _proxy;

        public Transmission(ITransmissionProxy proxy,
                            ITorrentFileInfoReader torrentFileInfoReader,
                            IHttpClient httpClient,
                            IConfigService configService,
                            IDiskProvider diskProvider,
                            IParsingService parsingService,
                            IRemotePathMappingService remotePathMappingService,
                            Logger logger)
            : base(torrentFileInfoReader, httpClient, configService, diskProvider, parsingService, remotePathMappingService, logger)
        {
            _proxy = proxy;
        }


        protected override String AddFromMagnetLink(RemoteEpisode remoteEpisode, String hash, String magnetLink)
        {
            _proxy.AddTorrentFromUrl(magnetLink, GetDownloadDirectory(), Settings);

            var isRecentEpisode = remoteEpisode.IsRecentEpisode();

            if (isRecentEpisode && Settings.RecentTvPriority == (int)TransmissionPriority.First ||
                !isRecentEpisode && Settings.OlderTvPriority == (int)TransmissionPriority.First)
            {
                _proxy.MoveTorrentToTopInQueue(hash, Settings);
            }

            return hash;
        }

        protected override String AddFromTorrentFile(RemoteEpisode remoteEpisode, String hash, String filename, Byte[] fileContent)
        {
            _proxy.AddTorrentFromData(fileContent, GetDownloadDirectory(), Settings);

            var isRecentEpisode = remoteEpisode.IsRecentEpisode();

            if (isRecentEpisode && Settings.RecentTvPriority == (int)TransmissionPriority.First ||
                !isRecentEpisode && Settings.OlderTvPriority == (int)TransmissionPriority.First)
            {
                _proxy.MoveTorrentToTopInQueue(hash, Settings);
            }

            return hash;
        }

        private String GetDownloadDirectory()
        {
            if (Settings.TvCategory.IsNullOrWhiteSpace()) return null;

            var config = _proxy.GetConfig(Settings);
            var destDir = (String)config.GetValueOrDefault("download-dir");

            return String.Format("{0}/.{1}", destDir, Settings.TvCategory);
        }

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            List<TransmissionTorrent> torrents;

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
                var outputPath = _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(torrent.DownloadDir));

                if (Settings.TvCategory.IsNotNullOrWhiteSpace())
                {
                    var directories = outputPath.FullPath.Split('\\', '/');
                    if (!directories.Contains(String.Format(".{0}", Settings.TvCategory))) continue;
                }

                var item = new DownloadClientItem();
                item.DownloadClientId = torrent.HashString.ToUpper();
                item.Category = Settings.TvCategory;
                item.Title = torrent.Name;

                item.DownloadClient = Definition.Name;
                item.DownloadTime = TimeSpan.FromSeconds(torrent.SecondsDownloading);
                item.Message = torrent.ErrorString;

                item.OutputPath = outputPath + torrent.Name;
                item.RemainingSize = torrent.LeftUntilDone;
                item.RemainingTime = TimeSpan.FromSeconds(torrent.Eta);
                item.TotalSize = torrent.TotalSize;

                if (!torrent.ErrorString.IsNullOrWhiteSpace())
                {
                    item.Status = DownloadItemStatus.Failed;
                }
                else if (torrent.Status == TransmissionTorrentStatus.Seeding || torrent.Status == TransmissionTorrentStatus.SeedingWait)
                {
                    item.Status = DownloadItemStatus.Completed;
                }
                else if (torrent.IsFinished && torrent.Status != TransmissionTorrentStatus.Check && torrent.Status != TransmissionTorrentStatus.CheckWait)
                {
                    item.Status = DownloadItemStatus.Completed;
                }
                else if (torrent.Status == TransmissionTorrentStatus.Queued)
                {
                    item.Status = DownloadItemStatus.Queued;
                }
                else
                {
                    item.Status = DownloadItemStatus.Downloading;
                }

                item.IsReadOnly = torrent.Status != TransmissionTorrentStatus.Stopped;

                items.Add(item);
            }

            return items;
        }

        public override void RemoveItem(String hash)
        {
            _proxy.RemoveTorrent(hash.ToLower(), false, Settings);
        }

        public override String RetryDownload(String hash)
        {
            throw new NotSupportedException();
        }

        public override DownloadClientStatus GetStatus()
        {
            var config = _proxy.GetConfig(Settings);
            var destDir = config.GetValueOrDefault("download-dir") as string;
            
            if (Settings.TvCategory.IsNotNullOrWhiteSpace())
            {
                destDir = String.Format("{0}/.{1}", destDir, Settings.TvCategory);
            }

            return new DownloadClientStatus
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost",
                OutputRootFolders = new List<OsPath> { _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(destDir)) }
            };
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
            if (failures.Any())
                return;
            failures.AddIfNotNull(TestGetTorrents());
        }

        private ValidationFailure TestConnection()
        {
            try
            {
                var versionResult =
                    Regex.Replace(_proxy.GetVersion(Settings), @"\([^)]*\)", "",
                        RegexOptions.IgnoreCase | RegexOptions.Multiline).Trim();
                var version = Version.Parse(versionResult);

                if (version < new Version(2, 40))
                {
                    return new ValidationFailure(string.Empty, "Transmission version not supported, should be 2.40 or higher.");
                }
            }
            catch (DownloadClientAuthenticationException ex)
            {
                _logger.ErrorException(ex.Message, ex);
                return new NzbDroneValidationFailure("Username", "Authentication failure")
                {
                    DetailedDescription = "Please verify your username and password. Also verify if the host running NzbDrone isn't blocked from accessing Transmission by WhiteList limitations in the Transmission configuration."
                };
            }
            catch (WebException ex)
            {
                _logger.ErrorException(ex.Message, ex);
                if (ex.Status == WebExceptionStatus.ConnectFailure)
                {
                    return new NzbDroneValidationFailure("Host", "Unable to connect")
                    {
                        DetailedDescription = "Please verify the hostname and port."
                    };
                }
                else
                {
                    return new NzbDroneValidationFailure(String.Empty, "Unknown exception: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException(ex.Message, ex);
                return new NzbDroneValidationFailure(String.Empty, "Unknown exception: " + ex.Message);
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
                return new NzbDroneValidationFailure(String.Empty, "Failed to get the list of torrents: " + ex.Message);
            }

            return null;
        }
    }
}
