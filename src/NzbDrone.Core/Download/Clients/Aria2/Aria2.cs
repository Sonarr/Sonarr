using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CookComputing.XmlRpc;
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

namespace NzbDrone.Core.Download.Clients.Aria2
{
    public class Aria2 : TorrentClientBase<Aria2Settings>
    {
        private readonly IAria2Proxy _proxy;

        public override string Name => "Aria2";

        public Aria2(IAria2Proxy proxy,
                        ITorrentFileInfoReader torrentFileInfoReader,
                        IHttpClient httpClient,
                        IConfigService configService,
                        IDiskProvider diskProvider,
                        IRemotePathMappingService remotePathMappingService,
                        Logger logger)
            : base(torrentFileInfoReader, httpClient, configService, diskProvider, remotePathMappingService, logger)
        {
            _proxy = proxy; ;
        }

        protected override string AddFromMagnetLink(RemoteEpisode remoteEpisode, string hash, string magnetLink)
        {
            var gid = _proxy.AddMagnet(Settings, magnetLink);

            var tries = 10;
            var retryDelay = 500;

            // Wait a bit for the magnet to be resolved.
            if (!WaitForTorrent(gid, hash, tries, retryDelay))
            {
                _logger.Warn("Aria2 could not resolve magnet within {0} seconds, download may remain stuck: {1}.", tries * retryDelay / 1000, magnetLink);

                return hash;
            }

            return hash;
        }

        protected override string AddFromTorrentFile(RemoteEpisode remoteEpisode, string hash, string filename, byte[] fileContent)
        {
            var gid = _proxy.AddTorrent(Settings, fileContent);

            var tries = 10;
            var retryDelay = 500;

            // Wait a bit for the magnet to be resolved.
            if (!WaitForTorrent(gid, hash, tries, retryDelay))
            {
                _logger.Warn("Aria2 could not resolve magnet within {0} seconds, download may remain stuck: {1}.", tries * retryDelay / 1000, filename);

                return hash;
            }

            return hash;
        }

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            var statuses = _proxy.GetStatuses(Settings);

            foreach(var status in statuses)
            {
                long completedLength = long.Parse(status.completedLength);
                long totalLength = long.Parse(status.totalLength);
                long uploadedLength = long.Parse(status.uploadLength);
                long downloadSpeed = long.Parse(status.downloadSpeed);

                var firstFile = status.files?.FirstOrDefault();

                DownloadItemStatus sta = DownloadItemStatus.Failed;

                string title = "";

                if(true == status.bittorrent?.ContainsKey("info") && ((XmlRpcStruct)status.bittorrent["info"]).ContainsKey("name"))
                {
                    title = ((XmlRpcStruct)status.bittorrent["info"])["name"].ToString();
                }

                switch (status.status)
                {
                    case "active":
                        sta = DownloadItemStatus.Downloading;
                        break;
                    case "waiting":
                        sta = DownloadItemStatus.Queued;
                        break;
                    case "paused":
                        sta = DownloadItemStatus.Paused;
                        break;
                    case "error":
                        sta = DownloadItemStatus.Failed;
                        break;
                    case "complete":
                        sta = DownloadItemStatus.Completed;
                        break;
                    case "removed":
                        sta = DownloadItemStatus.Failed;
                        break;
                }

                _logger.Debug($"- aria2 getstatus gid:'{status.gid}' sta:'{sta}' tot:{totalLength} comp:'{completedLength}'");

                yield return new DownloadClientItem()
                {
                    CanMoveFiles = false,
                    CanBeRemoved = true,
                    Category = null,
                    DownloadClientInfo = DownloadClientItemClientInfo.FromDownloadClient(this),
                    DownloadId = status.infoHash?.ToUpper(),
                    IsEncrypted = false,
                    Message = status.errorMessage,
                    OutputPath = _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(status.dir)),
                    RemainingSize = firstFile?.path?.Contains("[METADATA]") == true ? 1 : totalLength - completedLength,
                    RemainingTime = downloadSpeed != 0 ? new TimeSpan(0,0, (int)((totalLength - completedLength) / downloadSpeed)) : (TimeSpan?)null,
                    Removed = status.status == "removed",
                    SeedRatio = totalLength > 0 ? (double)uploadedLength / totalLength : 0,
                    Status = sta,
                    Title = title,
                    TotalSize = firstFile?.path?.Contains("[METADATA]") == true ? 1 : totalLength,
                };              
            }
        }

        public override void RemoveItem(string downloadId, bool deleteData)
        {
            //Aria2 doesn't support file deletion at this point: https://github.com/aria2/aria2/issues/728
            _proxy.RemoveTorrent(Settings, downloadId);
        }

        public override DownloadClientInfo GetStatus()
        {
            var destDir = _proxy.GetGlobals(Settings);

            return new DownloadClientInfo
            {
                IsLocalhost = Settings.URL.Contains("127.0.0.1") || Settings.URL.Contains("localhost"),
                OutputRootFolders = new List<OsPath> { _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(destDir["dir"])) }
            };
        }

        private bool WaitForTorrent(string gid, string hash, int tries, int retryDelay)
        {
            for (var i = 0; i < tries; i++)
            {
                var found = _proxy.GetFromGID(Settings, gid);

                if (found?.infoHash?.ToLower() == hash?.ToLower())
                {
                    return true;
                }

                Thread.Sleep(retryDelay);
            }

            _logger.Debug("Could not find hash {0} in {1} tries at {2} ms intervals.", hash, tries, retryDelay);

            return false;
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
            if (failures.HasErrors()) return;
        }

        private ValidationFailure TestConnection()
        {
            try
            {
                var version = _proxy.GetVersion(Settings);

                if (new Version(version) < new Version("1.34.0"))
                {
                    return new ValidationFailure(string.Empty, "Aria2 version should be at least 1.35.0. Version reported is {0}", version);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to test Aria2");

                return new NzbDroneValidationFailure("Host", "Unable to connect to Aria2")
                {
                    DetailedDescription = ex.Message
                };
            }

            return null;
        }
    }
}