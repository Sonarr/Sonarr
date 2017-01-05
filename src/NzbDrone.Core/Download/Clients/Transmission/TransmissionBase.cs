using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Transmission
{
    public abstract class TransmissionBase : TorrentClientBase<TransmissionSettings>
    {
        protected readonly ITransmissionProxy _proxy;

        public TransmissionBase(ITransmissionProxy proxy,
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

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            List<TransmissionTorrent> torrents;

            try
            {
                torrents = _proxy.GetTorrents(Settings);
            }
            catch (DownloadClientException ex)
            {
                _logger.Error(ex);
                return Enumerable.Empty<DownloadClientItem>();
            }

            var items = new List<DownloadClientItem>();

            foreach (var torrent in torrents)
            {
                // If totalsize == 0 the torrent is a magnet downloading metadata
                if (torrent.TotalSize == 0) continue;

                var outputPath = new OsPath(torrent.DownloadDir);

                if (Settings.TvDirectory.IsNotNullOrWhiteSpace())
                {
                    if (!new OsPath(Settings.TvDirectory).Contains(outputPath)) continue;
                }
                else if (Settings.TvCategory.IsNotNullOrWhiteSpace())
                {
                    var directories = outputPath.FullPath.Split('\\', '/');
                    if (!directories.Contains(Settings.TvCategory)) continue;
                }

                outputPath = _remotePathMappingService.RemapRemoteToLocal(Settings.Host, outputPath);

                var item = new DownloadClientItem();
                item.DownloadId = torrent.HashString.ToUpper();
                item.Category = Settings.TvCategory;
                item.Title = torrent.Name;

                item.DownloadClient = Definition.Name;

                item.OutputPath = GetOutputPath(outputPath, torrent);
                item.TotalSize = torrent.TotalSize;
                item.RemainingSize = torrent.LeftUntilDone;
                if (torrent.Eta >= 0)
                {
                    item.RemainingTime = TimeSpan.FromSeconds(torrent.Eta);
                }

                if (!torrent.ErrorString.IsNullOrWhiteSpace())
                {
                    item.Status = DownloadItemStatus.Warning;
                    item.Message = torrent.ErrorString;
                }
                else if (torrent.Status == TransmissionTorrentStatus.Seeding ||
                         torrent.Status == TransmissionTorrentStatus.SeedingWait)
                {
                    item.Status = DownloadItemStatus.Completed;
                }
                else if (torrent.IsFinished && torrent.Status != TransmissionTorrentStatus.Check &&
                         torrent.Status != TransmissionTorrentStatus.CheckWait)
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

        public override void RemoveItem(string downloadId, bool deleteData)
        {
            _proxy.RemoveTorrent(downloadId.ToLower(), deleteData, Settings);
        }

        public override DownloadClientStatus GetStatus()
        {
            var config = _proxy.GetConfig(Settings);
            var destDir = config.GetValueOrDefault("download-dir") as string;
            
            if (Settings.TvCategory.IsNotNullOrWhiteSpace())
            {
                destDir = string.Format("{0}/.{1}", destDir, Settings.TvCategory);
            }

            return new DownloadClientStatus
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost",
                OutputRootFolders = new List<OsPath> { _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(destDir)) }
            };
        }

        protected override string AddFromMagnetLink(RemoteEpisode remoteEpisode, string hash, string magnetLink)
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

        protected override string AddFromTorrentFile(RemoteEpisode remoteEpisode, string hash, string filename, byte[] fileContent)
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

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
            if (failures.Any()) return;
            failures.AddIfNotNull(TestGetTorrents());
        }

        protected virtual OsPath GetOutputPath(OsPath outputPath, TransmissionTorrent torrent)
        {
            return outputPath + torrent.Name;
        }

        protected string GetDownloadDirectory()
        {
            if (Settings.TvDirectory.IsNotNullOrWhiteSpace())
            {
                return Settings.TvDirectory;
            }
            else if (Settings.TvCategory.IsNotNullOrWhiteSpace())
            {
                var config = _proxy.GetConfig(Settings);
                var destDir = (string)config.GetValueOrDefault("download-dir");

                return string.Format("{0}/{1}", destDir.TrimEnd('/'), Settings.TvCategory);
            }
            else
            {
                return null;
            }
        }

        protected ValidationFailure TestConnection()
        {
            try
            {
                return ValidateVersion();
            }
            catch (DownloadClientAuthenticationException ex)
            {
                _logger.Error(ex);
                return new NzbDroneValidationFailure("Username", "Authentication failure")
                {
                    DetailedDescription = string.Format("Please verify your username and password. Also verify if the host running Sonarr isn't blocked from accessing {0} by WhiteList limitations in the {0} configuration.", Name)
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

        protected abstract ValidationFailure ValidateVersion();

        private ValidationFailure TestGetTorrents()
        {
            try
            {
                _proxy.GetTorrents(Settings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new NzbDroneValidationFailure(string.Empty, "Failed to get the list of torrents: " + ex.Message);
            }

            return null;
        }
    }
}