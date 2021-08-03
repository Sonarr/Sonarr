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
using NzbDrone.Core.Download.Clients.DownloadStation.Proxies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.DownloadStation
{
    public class UsenetDownloadStation : UsenetClientBase<DownloadStationSettings>
    {
        protected readonly IDownloadStationInfoProxy _dsInfoProxy;
        protected readonly IDownloadStationTaskProxySelector _dsTaskProxySelector;
        protected readonly ISharedFolderResolver _sharedFolderResolver;
        protected readonly ISerialNumberProvider _serialNumberProvider;
        protected readonly IFileStationProxy _fileStationProxy;

        public UsenetDownloadStation(ISharedFolderResolver sharedFolderResolver,
                                     ISerialNumberProvider serialNumberProvider,
                                     IFileStationProxy fileStationProxy,
                                     IDownloadStationInfoProxy dsInfoProxy,
                                     IDownloadStationTaskProxySelector dsTaskProxySelector,
                                     IHttpClient httpClient,
                                     IConfigService configService,
                                     IDiskProvider diskProvider,
                                     IRemotePathMappingService remotePathMappingService,
                                     IValidateNzbs nzbValidationService,
                                     Logger logger)
            : base(httpClient, configService, diskProvider, remotePathMappingService, nzbValidationService, logger)
        {
            _dsInfoProxy = dsInfoProxy;
            _dsTaskProxySelector = dsTaskProxySelector;
            _fileStationProxy = fileStationProxy;
            _sharedFolderResolver = sharedFolderResolver;
            _serialNumberProvider = serialNumberProvider;
        }

        public override string Name => "Download Station";

        public override ProviderMessage Message => new ProviderMessage("Sonarr is unable to connect to Download Station if 2-Factor Authentication is enabled on your DSM account", ProviderMessageType.Warning);

        private IDownloadStationTaskProxy DsTaskProxy => _dsTaskProxySelector.GetProxy(Settings);

        protected IEnumerable<DownloadStationTask> GetTasks()
        {
            return DsTaskProxy.GetTasks(Settings).Where(v => v.Type.ToLower() == DownloadStationTaskType.NZB.ToString().ToLower());
        }

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            var nzbTasks = GetTasks();
            var serialNumber = _serialNumberProvider.GetSerialNumber(Settings);

            var items = new List<DownloadClientItem>();

            long totalRemainingSize = 0;
            long globalSpeed = nzbTasks.Where(t => t.Status == DownloadStationTaskStatus.Downloading)
                                       .Select(GetDownloadSpeed)
                                       .Sum();

            foreach (var nzb in nzbTasks)
            {
                var outputPath = new OsPath($"/{nzb.Additional.Detail["destination"]}");

                var taskRemainingSize = GetRemainingSize(nzb);

                if (nzb.Status != DownloadStationTaskStatus.Paused)
                {
                    totalRemainingSize += taskRemainingSize;
                }

                if (Settings.TvDirectory.IsNotNullOrWhiteSpace())
                {
                    if (!new OsPath($"/{Settings.TvDirectory}").Contains(outputPath))
                    {
                        continue;
                    }
                }
                else if (Settings.TvCategory.IsNotNullOrWhiteSpace())
                {
                    var directories = outputPath.FullPath.Split('\\', '/');
                    if (!directories.Contains(Settings.TvCategory))
                    {
                        continue;
                    }
                }

                var item = new DownloadClientItem()
                {
                    Category = Settings.TvCategory,
                    DownloadClientInfo = DownloadClientItemClientInfo.FromDownloadClient(this),
                    DownloadId = CreateDownloadId(nzb.Id, serialNumber),
                    Title = nzb.Title,
                    TotalSize = nzb.Size,
                    RemainingSize = taskRemainingSize,
                    Status = GetStatus(nzb),
                    Message = GetMessage(nzb),
                    CanBeRemoved = true,
                    CanMoveFiles = true
                };

                if (item.Status != DownloadItemStatus.Paused)
                {
                    item.RemainingTime = GetRemainingTime(totalRemainingSize, globalSpeed);
                }

                if (item.Status == DownloadItemStatus.Completed || item.Status == DownloadItemStatus.Failed)
                {
                    item.OutputPath = GetOutputPath(outputPath, nzb, serialNumber);
                }

                items.Add(item);
            }

            return items;
        }

        protected OsPath GetOutputPath(OsPath outputPath, DownloadStationTask task, string serialNumber)
        {
            var fullPath = _sharedFolderResolver.RemapToFullPath(outputPath, Settings, serialNumber);

            var remotePath = _remotePathMappingService.RemapRemoteToLocal(Settings.Host, fullPath);

            var finalPath = remotePath + task.Title;

            return finalPath;
        }

        public override DownloadClientInfo GetStatus()
        {
            try
            {
                var serialNumber = _serialNumberProvider.GetSerialNumber(Settings);

                // Download station returns the path without the leading `/`, but the leading
                // slash is required to get the full path back from download station.
                var path = new OsPath($"/{GetDownloadDirectory()}");

                var fullPath = _sharedFolderResolver.RemapToFullPath(path, Settings, serialNumber);

                return new DownloadClientInfo
                {
                    IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost",
                    OutputRootFolders = new List<OsPath> { _remotePathMappingService.RemapRemoteToLocal(Settings.Host, fullPath) }
                };
            }
            catch (DownloadClientException e)
            {
                _logger.Debug(e, "Failed to get config from Download Station");

                throw;
            }
        }

        public override void RemoveItem(DownloadClientItem item, bool deleteData)
        {
            if (deleteData)
            {
                DeleteItemData(item);
            }

            DsTaskProxy.RemoveTask(ParseDownloadId(item.DownloadId), Settings);
            _logger.Debug("{0} removed correctly", item.DownloadId);
        }

        protected override string AddFromNzbFile(RemoteEpisode remoteEpisode, string filename, byte[] fileContent)
        {
            var hashedSerialNumber = _serialNumberProvider.GetSerialNumber(Settings);

            DsTaskProxy.AddTaskFromData(fileContent, filename, GetDownloadDirectory(), Settings);

            var items = GetTasks().Where(t => t.Additional.Detail["uri"] == filename);

            var item = items.SingleOrDefault();

            if (item != null)
            {
                _logger.Debug("{0} added correctly", remoteEpisode);
                return CreateDownloadId(item.Id, hashedSerialNumber);
            }

            _logger.Debug("No such task {0} in Download Station", filename);

            throw new DownloadClientException("Failed to add NZB task to Download Station");
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
            if (failures.HasErrors())
            {
                return;
            }

            failures.AddIfNotNull(TestOutputPath());
            failures.AddIfNotNull(TestGetNZB());
        }

        protected ValidationFailure TestOutputPath()
        {
            try
            {
                var downloadDir = GetDefaultDir();

                if (downloadDir == null)
                {
                    return new NzbDroneValidationFailure(nameof(Settings.TvDirectory), "No default destination")
                    {
                        DetailedDescription = $"You must login into your Diskstation as {Settings.Username} and manually set it up into DownloadStation settings under BT/HTTP/FTP/NZB -> Location."
                    };
                }

                downloadDir = GetDownloadDirectory();

                if (downloadDir != null)
                {
                    var sharedFolder = downloadDir.Split('\\', '/')[0];
                    var fieldName = Settings.TvDirectory.IsNotNullOrWhiteSpace() ? nameof(Settings.TvDirectory) : nameof(Settings.TvCategory);

                    var folderInfo = _fileStationProxy.GetInfoFileOrDirectory($"/{downloadDir}", Settings);

                    if (folderInfo.Additional == null)
                    {
                        return new NzbDroneValidationFailure(fieldName, $"Shared folder does not exist")
                        {
                            DetailedDescription = $"The Diskstation does not have a Shared Folder with the name '{sharedFolder}', are you sure you specified it correctly?"
                        };
                    }

                    if (!folderInfo.IsDir)
                    {
                        return new NzbDroneValidationFailure(fieldName, $"Folder does not exist")
                        {
                            DetailedDescription = $"The folder '{downloadDir}' does not exist, it must be created manually inside the Shared Folder '{sharedFolder}'."
                        };
                    }
                }

                return null;
            }
            catch (DownloadClientAuthenticationException ex)
            {
                // User could not have permission to access to downloadstation
                _logger.Error(ex, ex.Message);
                return new NzbDroneValidationFailure(string.Empty, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error testing Usenet Download Station");
                return new NzbDroneValidationFailure(string.Empty, $"Unknown exception: {ex.Message}");
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
                _logger.Error(ex, ex.Message);
                return new NzbDroneValidationFailure("Username", "Authentication failure")
                {
                    DetailedDescription = $"Please verify your username and password. Also verify if the host running Sonarr isn't blocked from accessing {Name} by WhiteList limitations in the {Name} configuration."
                };
            }
            catch (WebException ex)
            {
                _logger.Error(ex, "Unable to connect to Usenet Download Station");

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
                _logger.Error(ex, "Error testing Torrent Download Station");

                return new NzbDroneValidationFailure("Host", "Unable to connect to Usenet Download Station")
                       {
                           DetailedDescription = ex.Message
                       };
            }
        }

        protected ValidationFailure ValidateVersion()
        {
            var info = DsTaskProxy.GetApiInfo(Settings);

            _logger.Debug("Download Station api version information: Min {0} - Max {1}", info.MinVersion, info.MaxVersion);

            if (info.MinVersion > 2 || info.MaxVersion < 2)
            {
                return new ValidationFailure(string.Empty, $"Download Station API version not supported, should be at least 2. It supports from {info.MinVersion} to {info.MaxVersion}");
            }

            return null;
        }

        protected string GetMessage(DownloadStationTask task)
        {
            if (task.StatusExtra != null)
            {
                if (task.Status == DownloadStationTaskStatus.Extracting)
                {
                    return $"Extracting: {int.Parse(task.StatusExtra["unzip_progress"])}%";
                }

                if (task.Status == DownloadStationTaskStatus.Error)
                {
                    return task.StatusExtra["error_detail"];
                }
            }

            return null;
        }

        protected DownloadItemStatus GetStatus(DownloadStationTask task)
        {
            switch (task.Status)
            {
                case DownloadStationTaskStatus.Unknown:
                case DownloadStationTaskStatus.Waiting:
                case DownloadStationTaskStatus.FilehostingWaiting:
                    return task.Size == 0 || GetRemainingSize(task) > 0 ? DownloadItemStatus.Queued : DownloadItemStatus.Completed;
                case DownloadStationTaskStatus.Paused:
                    return DownloadItemStatus.Paused;
                case DownloadStationTaskStatus.Finished:
                case DownloadStationTaskStatus.Seeding:
                    return DownloadItemStatus.Completed;
                case DownloadStationTaskStatus.Error:
                    return DownloadItemStatus.Failed;
            }

            return DownloadItemStatus.Downloading;
        }

        protected long GetRemainingSize(DownloadStationTask task)
        {
            var downloadedString = task.Additional.Transfer["size_downloaded"];
            long downloadedSize;

            if (downloadedString.IsNullOrWhiteSpace() || !long.TryParse(downloadedString, out downloadedSize))
            {
                _logger.Debug("Task {0} has invalid size_downloaded: {1}", task.Title, downloadedString);
                downloadedSize = 0;
            }

            return task.Size - Math.Max(0, downloadedSize);
        }

        protected long GetDownloadSpeed(DownloadStationTask task)
        {
            var speedString = task.Additional.Transfer["speed_download"];
            long downloadSpeed;

            if (speedString.IsNullOrWhiteSpace() || !long.TryParse(speedString, out downloadSpeed))
            {
                _logger.Debug("Task {0} has invalid speed_download: {1}", task.Title, speedString);
                downloadSpeed = 0;
            }

            return Math.Max(downloadSpeed, 0);
        }

        protected TimeSpan? GetRemainingTime(long remainingSize, long downloadSpeed)
        {
            if (downloadSpeed > 0)
            {
                return TimeSpan.FromSeconds(remainingSize / downloadSpeed);
            }
            else
            {
                return null;
            }
        }

        protected ValidationFailure TestGetNZB()
        {
            try
            {
                GetItems();
                return null;
            }
            catch (Exception ex)
            {
                return new NzbDroneValidationFailure(string.Empty, "Failed to get the list of NZBs: " + ex.Message);
            }
        }

        protected string ParseDownloadId(string id)
        {
            return id.Split(':')[1];
        }

        protected string CreateDownloadId(string id, string hashedSerialNumber)
        {
            return $"{hashedSerialNumber}:{id}";
        }

        protected string GetDefaultDir()
        {
            var config = _dsInfoProxy.GetConfig(Settings);

            var path = config["default_destination"] as string;

            return path;
        }

        protected string GetDownloadDirectory()
        {
            if (Settings.TvDirectory.IsNotNullOrWhiteSpace())
            {
                return Settings.TvDirectory.TrimStart('/');
            }

            var destDir = GetDefaultDir();

            if (Settings.TvCategory.IsNotNullOrWhiteSpace())
            {
                return $"{destDir.TrimEnd('/')}/{Settings.TvCategory}";
            }

            return destDir.TrimEnd('/');
        }
    }
}
