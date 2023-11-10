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
using NzbDrone.Core.Localization;
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
                                     Logger logger,
                                     ILocalizationService localizationService)
            : base(httpClient, configService, diskProvider, remotePathMappingService, nzbValidationService, logger, localizationService)
        {
            _dsInfoProxy = dsInfoProxy;
            _dsTaskProxySelector = dsTaskProxySelector;
            _fileStationProxy = fileStationProxy;
            _sharedFolderResolver = sharedFolderResolver;
            _serialNumberProvider = serialNumberProvider;
        }

        public override string Name => "Download Station";

        public override ProviderMessage Message => new ProviderMessage(_localizationService.GetLocalizedString("DownloadClientDownloadStationProviderMessage"), ProviderMessageType.Warning);

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
            var globalSpeed = nzbTasks.Where(t => t.Status == DownloadStationTaskStatus.Downloading)
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
                    return new NzbDroneValidationFailure(nameof(Settings.TvDirectory), "DownloadClientDownloadStationValidationNoDefaultDestination")
                    {
                        DetailedDescription = _localizationService.GetLocalizedString("DownloadClientDownloadStationValidationNoDefaultDestinationDetail", new Dictionary<string, object> { { "username", Settings.Username } })
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
                        return new NzbDroneValidationFailure(fieldName, _localizationService.GetLocalizedString("DownloadClientDownloadStationValidationSharedFolderMissing"))
                        {
                            DetailedDescription = _localizationService.GetLocalizedString("DownloadClientDownloadStationValidationSharedFolderMissingDetail", new Dictionary<string, object> { { "sharedFolder", sharedFolder } })
                        };
                    }

                    if (!folderInfo.IsDir)
                    {
                        return new NzbDroneValidationFailure(fieldName, _localizationService.GetLocalizedString("DownloadClientDownloadStationValidationFolderMissing"))
                        {
                            DetailedDescription = _localizationService.GetLocalizedString("DownloadClientDownloadStationValidationFolderMissingDetail", new Dictionary<string, object> { { "downloadDir", downloadDir }, { "sharedFolder", sharedFolder } })
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
                return new NzbDroneValidationFailure(string.Empty, _localizationService.GetLocalizedString("DownloadClientValidationUnknownException", new Dictionary<string, object> { { "exception", ex.Message } }));
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
                return new NzbDroneValidationFailure("Username", _localizationService.GetLocalizedString("DownloadClientValidationAuthenticationFailure"))
                {
                    DetailedDescription = _localizationService.GetLocalizedString("DownloadClientValidationAuthenticationFailureDetail", new Dictionary<string, object> { { "clientName", Name } })
                };
            }
            catch (WebException ex)
            {
                _logger.Error(ex, "Unable to connect to Usenet Download Station");

                if (ex.Status == WebExceptionStatus.ConnectFailure)
                {
                    return new NzbDroneValidationFailure("Host", _localizationService.GetLocalizedString("DownloadClientValidationUnableToConnect", new Dictionary<string, object> { { "clientName", Name } }))
                    {
                        DetailedDescription = _localizationService.GetLocalizedString("DownloadClientValidationUnableToConnectDetail")
                    };
                }

                return new NzbDroneValidationFailure(string.Empty, _localizationService.GetLocalizedString("DownloadClientValidationUnknownException", new Dictionary<string, object> { { "exception", ex.Message } }));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error testing Torrent Download Station");

                return new NzbDroneValidationFailure("Host", _localizationService.GetLocalizedString("DownloadClientValidationUnableToConnect", new Dictionary<string, object> { { "clientName", Name } }))
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
                return new ValidationFailure(string.Empty,
                    _localizationService.GetLocalizedString("DownloadClientDownloadStationValidationApiVersion",
                        new Dictionary<string, object>
                        {
                            { "requiredVersion", 2 }, { "minVersion", info.MinVersion }, { "maxVersion", info.MaxVersion }
                        }));
            }

            return null;
        }

        protected string GetMessage(DownloadStationTask task)
        {
            if (task.StatusExtra != null)
            {
                if (task.Status == DownloadStationTaskStatus.Extracting)
                {
                    return _localizationService.GetLocalizedString("DownloadStationStatusExtracting",
                        new Dictionary<string, object>
                            { { "progress", int.Parse(task.StatusExtra["unzip_progress"]) } });
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

            if (downloadedString.IsNullOrWhiteSpace() || !long.TryParse(downloadedString, out var downloadedSize))
            {
                _logger.Debug("Task {0} has invalid size_downloaded: {1}", task.Title, downloadedString);
                downloadedSize = 0;
            }

            return task.Size - Math.Max(0, downloadedSize);
        }

        protected long GetDownloadSpeed(DownloadStationTask task)
        {
            var speedString = task.Additional.Transfer["speed_download"];

            if (speedString.IsNullOrWhiteSpace() || !long.TryParse(speedString, out var downloadSpeed))
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
                return new NzbDroneValidationFailure(string.Empty, _localizationService.GetLocalizedString("DownloadClientValidationTestNzbs", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
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
