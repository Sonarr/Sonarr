using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
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
    public class Transmission : TorrentClientBase<TransmissionSettings>
    {
        private readonly ITransmissionProxy _proxy;

        public Transmission(ITransmissionProxy proxy,
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


        protected override String AddFromMagnetLink(RemoteEpisode remoteEpisode, String hash, String magnetLink)
        {
            _proxy.AddTorrentFromUrl(magnetLink, GetDownloadDirectory(Settings.TvCategory), Settings);

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
            _proxy.AddTorrentFromData(fileContent, GetDownloadDirectory(Settings.TvCategory), Settings);

            var isRecentEpisode = remoteEpisode.IsRecentEpisode();

            if (isRecentEpisode && Settings.RecentTvPriority == (int)TransmissionPriority.First ||
                !isRecentEpisode && Settings.OlderTvPriority == (int)TransmissionPriority.First)
            {
                _proxy.MoveTorrentToTopInQueue(hash, Settings);
            }

            return hash;
        }

        protected override String AddFromMagnetLink(RemoteMovie remoteMovie, String hash, String magnetLink)
        {
            _proxy.AddTorrentFromUrl(magnetLink, GetDownloadDirectory(Settings.MovieCategory), Settings);

            var isRecentMovie = remoteMovie.IsRecentMovie();

            if (isRecentMovie && Settings.RecentMoviePriority == (int)TransmissionPriority.First ||
                !isRecentMovie && Settings.OlderMoviePriority == (int)TransmissionPriority.First)
            {
                _proxy.MoveTorrentToTopInQueue(hash, Settings);
            }

            return hash;
        }

        protected override String AddFromTorrentFile(RemoteMovie remoteMovie, String hash, String filename, Byte[] fileContent)
        {
            _proxy.AddTorrentFromData(fileContent, GetDownloadDirectory(Settings.MovieCategory), Settings);

            var isRecentMovie = remoteMovie.IsRecentMovie();

            if (isRecentMovie && Settings.RecentMoviePriority == (int)TransmissionPriority.First ||
                !isRecentMovie && Settings.OlderMoviePriority == (int)TransmissionPriority.First)
            {
                _proxy.MoveTorrentToTopInQueue(hash, Settings);
            }

            return hash;
        }


        private String GetDownloadDirectory(string category)
        {
            if (category.IsNullOrWhiteSpace()) return null;

            var config = _proxy.GetConfig(Settings);
            var destDir = (String)config.GetValueOrDefault("download-dir");

            return string.Format("{0}/{1}", destDir.TrimEnd('/'), category);
        }

        public override string Name
        {
            get
            {
                return "Transmission";
            }
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
                var itemType = DownloadItemType.Unknown;
                var category = "sonarr";
                var directories = outputPath.FullPath.Split('\\', '/');

                if (Settings.TvCategory.IsNotNullOrWhiteSpace() && directories.Contains(String.Format("{0}", Settings.TvCategory)))
                {
                    itemType = DownloadItemType.Series;
                    category = Settings.TvCategory;
                }
                else if (Settings.MovieCategory.IsNotNullOrWhiteSpace() && directories.Contains(String.Format("{0}", Settings.MovieCategory)))
                {
                    itemType = DownloadItemType.Movie;
                    category = Settings.MovieCategory;
                }
                else if (Settings.TvCategory.IsNotNullOrWhiteSpace() && Settings.MovieCategory.IsNotNullOrWhiteSpace())
                {
                    continue;
                }

                var item = new DownloadClientItem();
                item.DownloadId = torrent.HashString.ToUpper();
                item.Category = category;
                item.DownloadType = itemType;
                item.Title = torrent.Name;

                item.DownloadClient = Definition.Name;

                item.OutputPath = outputPath + torrent.Name;
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

        public override void RemoveItem(string downloadId, bool deleteData)
        {
            _proxy.RemoveTorrent(downloadId.ToLower(), deleteData, Settings);
        }

        public override DownloadClientStatus GetStatus()
        {
            var config = _proxy.GetConfig(Settings);
            var tvDestDir = config.GetValueOrDefault("download-dir") as string;
            var movieDestDir = config.GetValueOrDefault("download-dir") as string;

            if (Settings.TvCategory.IsNotNullOrWhiteSpace())
            {
                tvDestDir = String.Format("{0}/.{1}", tvDestDir, Settings.TvCategory);
            }

            if (Settings.MovieCategory.IsNotNullOrWhiteSpace())
            {
                movieDestDir = String.Format("{0}/.{1}", movieDestDir, Settings.MovieCategory);
            }

            if (tvDestDir == movieDestDir)
                movieDestDir = null;

            var returnValue = new DownloadClientStatus
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost",
                OutputRootFolders = new List<OsPath> { _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(tvDestDir)) }
            };

            if (movieDestDir.IsNotNullOrWhiteSpace())
            {
                returnValue.OutputRootFolders.Add(_remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(tvDestDir)));
            }

            return returnValue;
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
            if (failures.Any()) return;
            failures.AddIfNotNull(TestGetTorrents());
        }

        private ValidationFailure TestConnection()
        {
            try
            {
                var versionString = _proxy.GetVersion(Settings);

                _logger.Debug("Transmission version information: {0}", versionString);

                var versionResult = Regex.Match(versionString, @"(?<!\(|(\d|\.)+)(\d|\.)+(?!\)|(\d|\.)+)").Value;
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
                    DetailedDescription = "Please verify your username and password. Also verify if the host running Sonarr isn't blocked from accessing Transmission by WhiteList limitations in the Transmission configuration."
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
                return new NzbDroneValidationFailure(String.Empty, "Unknown exception: " + ex.Message);
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
