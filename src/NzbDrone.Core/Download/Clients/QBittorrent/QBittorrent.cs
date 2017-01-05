using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NLog;
using NzbDrone.Core.Validation;
using FluentValidation.Results;
using System.Net;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;

namespace NzbDrone.Core.Download.Clients.QBittorrent
{
    public class QBittorrent : TorrentClientBase<QBittorrentSettings>
    {
        private readonly IQBittorrentProxy _proxy;

        public QBittorrent(IQBittorrentProxy proxy,
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

        protected override string AddFromMagnetLink(RemoteEpisode remoteEpisode, string hash, string magnetLink)
        {
            _proxy.AddTorrentFromUrl(magnetLink, Settings);

            if (Settings.TvCategory.IsNotNullOrWhiteSpace())
            {
                _proxy.SetTorrentLabel(hash.ToLower(), Settings.TvCategory, Settings);
            }

            var isRecentEpisode = remoteEpisode.IsRecentEpisode();

            if (isRecentEpisode && Settings.RecentTvPriority == (int)QBittorrentPriority.First ||
                !isRecentEpisode && Settings.OlderTvPriority == (int)QBittorrentPriority.First)
            {
                _proxy.MoveTorrentToTopInQueue(hash.ToLower(), Settings);
            }

            return hash;
        }

        protected override string AddFromTorrentFile(RemoteEpisode remoteEpisode, string hash, string filename, Byte[] fileContent)
        {
            _proxy.AddTorrentFromFile(filename, fileContent, Settings);

            if (Settings.TvCategory.IsNotNullOrWhiteSpace())
            {
                _proxy.SetTorrentLabel(hash.ToLower(), Settings.TvCategory, Settings);
            }

            var isRecentEpisode = remoteEpisode.IsRecentEpisode();

            if (isRecentEpisode && Settings.RecentTvPriority == (int)QBittorrentPriority.First ||
                !isRecentEpisode && Settings.OlderTvPriority == (int)QBittorrentPriority.First)
            {
                _proxy.MoveTorrentToTopInQueue(hash.ToLower(), Settings);
            }

            return hash;
        }

        public override string Name => "qBittorrent";

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            QBittorrentPreferences config;
            List<QBittorrentTorrent> torrents;

            try
            {
                config = _proxy.GetConfig(Settings);
                torrents = _proxy.GetTorrents(Settings);
            }
            catch (DownloadClientException ex)
            {
                _logger.Error(ex);
                return Enumerable.Empty<DownloadClientItem>();
            }

            var queueItems = new List<DownloadClientItem>();

            foreach (var torrent in torrents)
            {
                var item = new DownloadClientItem();
                item.DownloadId = torrent.Hash.ToUpper();
                item.Category = torrent.Category.IsNotNullOrWhiteSpace() ? torrent.Category : torrent.Label;
                item.Title = torrent.Name;
                item.TotalSize = torrent.Size;
                item.DownloadClient = Definition.Name;
                item.RemainingSize = (long)(torrent.Size * (1.0 - torrent.Progress));
                item.RemainingTime = TimeSpan.FromSeconds(torrent.Eta);

                item.OutputPath = _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(torrent.SavePath));

                // Avoid removing torrents that haven't reached the global max ratio.
                // Removal also requires the torrent to be paused, in case a higher max ratio was set on the torrent itself (which is not exposed by the api).
                item.IsReadOnly = (config.MaxRatioEnabled && config.MaxRatio > torrent.Ratio) || torrent.State != "pausedUP";

                if (!item.OutputPath.IsEmpty && item.OutputPath.FileName != torrent.Name)
                {
                    item.OutputPath += torrent.Name;
                }

                switch (torrent.State)
                {
                    case "error": // some error occurred, applies to paused torrents
                        item.Status = DownloadItemStatus.Failed;
                        item.Message = "QBittorrent is reporting an error";
                        break;

                    case "pausedDL": // torrent is paused and has NOT finished downloading
                        item.Status = DownloadItemStatus.Paused;
                        break;

                    case "queuedDL": // queuing is enabled and torrent is queued for download
                    case "checkingDL": // same as checkingUP, but torrent has NOT finished downloading
                        item.Status = DownloadItemStatus.Queued;
                        break;

                    case "pausedUP": // torrent is paused and has finished downloading
                    case "uploading": // torrent is being seeded and data is being transfered
                    case "stalledUP": // torrent is being seeded, but no connection were made
                    case "queuedUP": // queuing is enabled and torrent is queued for upload
                    case "checkingUP": // torrent has finished downloading and is being checked
                        item.Status = DownloadItemStatus.Completed;
                        item.RemainingTime = TimeSpan.Zero; // qBittorrent sends eta=8640000 for completed torrents
                        break;

                    case "stalledDL": // torrent is being downloaded, but no connection were made
                        item.Status = DownloadItemStatus.Warning;
                        item.Message = "The download is stalled with no connections";
                        break;

                    case "downloading": // torrent is being downloaded and data is being transfered
                    default: // new status in API? default to downloading
                        item.Status = DownloadItemStatus.Downloading;
                        break;
                }

                queueItems.Add(item);
            }

            return queueItems;
        }

        public override void RemoveItem(string hash, bool deleteData)
        {
            _proxy.RemoveTorrent(hash.ToLower(), deleteData, Settings);
        }

        public override DownloadClientStatus GetStatus()
        {
            var config = _proxy.GetConfig(Settings);

            var destDir = new OsPath(config.SavePath);

            return new DownloadClientStatus
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost",
                OutputRootFolders = new List<OsPath> { _remotePathMappingService.RemapRemoteToLocal(Settings.Host, destDir) }
            };
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
                var version = _proxy.GetVersion(Settings);
                if (version < 5)
                {
                    // API version 5 introduced the "save_path" property in /query/torrents
                    return new NzbDroneValidationFailure("Host", "Unsupported client version")
                    {
                        DetailedDescription = "Please upgrade to qBittorrent version 3.2.4 or higher."
                    };
                }
                else if (version < 6)
                {
                    // API version 6 introduced support for labels
                    if (Settings.TvCategory.IsNotNullOrWhiteSpace())
                    {
                        return new NzbDroneValidationFailure("Category", "Category is not supported")
                        {
                            DetailedDescription = "Labels are not supported until qBittorrent version 3.3.0. Please upgrade or try again with an empty Category."
                        };
                    }
                }
                else if (Settings.TvCategory.IsNullOrWhiteSpace())
                {
                    // warn if labels are supported, but category is not provided
                    return new NzbDroneValidationFailure("TvCategory", "Category is recommended")
                    {
                        IsWarning = true,
                        DetailedDescription = "Sonarr will not attempt to import completed downloads without a category."
                    };
                }

                // Complain if qBittorrent is configured to remove torrents on max ratio
                var config = _proxy.GetConfig(Settings);
                if (config.MaxRatioEnabled && config.RemoveOnMaxRatio)
                {
                    return new NzbDroneValidationFailure(String.Empty, "QBittorrent is configured to remove torrents when they reach their Share Ratio Limit")
                    {
                        DetailedDescription = "Sonarr will be unable to perform Completed Download Handling as configured. You can fix this in qBittorrent ('Tools -> Options...' in the menu) by changing 'Options -> BitTorrent -> Share Ratio Limiting' from 'Remove them' to 'Pause them'."
                    };
                }
            }
            catch (DownloadClientAuthenticationException ex)
            {
                _logger.Error(ex);
                return new NzbDroneValidationFailure("Username", "Authentication failure")
                {
                    DetailedDescription = "Please verify your username and password."
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
                return new NzbDroneValidationFailure(String.Empty, "Unknown exception: " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
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
                _logger.Error(ex);
                return new NzbDroneValidationFailure(String.Empty, "Failed to get the list of torrents: " + ex.Message);
            }

            return null;
        }
    }
}
