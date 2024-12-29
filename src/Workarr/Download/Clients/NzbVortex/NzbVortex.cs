using FluentValidation.Results;
using NLog;
using Workarr.Configuration;
using Workarr.Disk;
using Workarr.Http;
using Workarr.Localization;
using Workarr.Parser.Model;
using Workarr.RemotePathMappings;
using Workarr.Validation;
using EnumerableExtensions = Workarr.Extensions.EnumerableExtensions;
using StringExtensions = Workarr.Extensions.StringExtensions;

namespace Workarr.Download.Clients.NzbVortex
{
    public class NzbVortex : UsenetClientBase<NzbVortexSettings>
    {
        private readonly INzbVortexProxy _proxy;

        public NzbVortex(INzbVortexProxy proxy,
                       IHttpClient httpClient,
                       IConfigService configService,
                       IDiskProvider diskProvider,
                       IRemotePathMappingService remotePathMappingService,
                       IValidateNzbs nzbValidationService,
                       Logger logger,
                       ILocalizationService localizationService)
            : base(httpClient, configService, diskProvider, remotePathMappingService, nzbValidationService, logger, localizationService)
        {
            _proxy = proxy;
        }

        protected override string AddFromNzbFile(RemoteEpisode remoteEpisode, string filename, byte[] fileContent)
        {
            var priority = remoteEpisode.IsRecentEpisode() ? Settings.RecentTvPriority : Settings.OlderTvPriority;

            var response = _proxy.DownloadNzb(fileContent, filename, priority, Settings);

            if (response == null)
            {
                throw new DownloadClientException("Failed to add nzb {0}", filename);
            }

            return response;
        }

        public override string Name => "NZBVortex";

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            var vortexQueue = _proxy.GetQueue(30, Settings);

            var queueItems = new List<DownloadClientItem>();

            foreach (var vortexQueueItem in vortexQueue)
            {
                var queueItem = new DownloadClientItem();

                queueItem.DownloadClientInfo = DownloadClientItemClientInfo.FromDownloadClient(this, false);
                queueItem.DownloadId = vortexQueueItem.AddUUID ?? vortexQueueItem.Id.ToString();
                queueItem.Category = vortexQueueItem.GroupName;
                queueItem.Title = vortexQueueItem.UiTitle;
                queueItem.TotalSize = vortexQueueItem.TotalDownloadSize;
                queueItem.RemainingSize = vortexQueueItem.TotalDownloadSize - vortexQueueItem.DownloadedSize;
                queueItem.RemainingTime = null;
                queueItem.CanBeRemoved = true;
                queueItem.CanMoveFiles = true;

                if (vortexQueueItem.IsPaused)
                {
                    queueItem.Status = DownloadItemStatus.Paused;
                }
                else
                {
                    switch (vortexQueueItem.State)
                {
                    case NzbVortexStateType.Waiting:
                        queueItem.Status = DownloadItemStatus.Queued;
                        break;
                    case NzbVortexStateType.Done:
                        queueItem.Status = DownloadItemStatus.Completed;
                        break;
                    case NzbVortexStateType.UncompressFailed:
                    case NzbVortexStateType.CheckFailedDataCorrupt:
                    case NzbVortexStateType.BadlyEncoded:
                        queueItem.Status = DownloadItemStatus.Failed;
                        break;
                    default:
                        queueItem.Status = DownloadItemStatus.Downloading;
                        break;
                }
                }

                queueItem.OutputPath = GetOutputPath(vortexQueueItem, queueItem);

                if (vortexQueueItem.State == NzbVortexStateType.PasswordRequest)
                {
                    queueItem.IsEncrypted = true;
                }

                if (queueItem.Status == DownloadItemStatus.Completed)
                {
                    queueItem.RemainingTime = TimeSpan.Zero;
                }

                queueItems.Add(queueItem);
            }

            return queueItems;
        }

        public override void RemoveItem(DownloadClientItem item, bool deleteData)
        {
            // Try to find the download by numerical ID, otherwise try by AddUUID

            if (int.TryParse(item.DownloadId, out var id))
            {
                _proxy.Remove(id, deleteData, Settings);
            }
            else
            {
                var queue = _proxy.GetQueue(30, Settings);
                var queueItem = queue.FirstOrDefault(c => c.AddUUID == item.DownloadId);

                if (queueItem != null)
                {
                    _proxy.Remove(queueItem.Id, deleteData, Settings);
                }
            }
        }

        protected List<NzbVortexGroup> GetGroups()
        {
            return _proxy.GetGroups(Settings);
        }

        public override DownloadClientInfo GetStatus()
        {
            var status = new DownloadClientInfo
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost"
            };

            return status;
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            EnumerableExtensions.AddIfNotNull(failures, TestConnection());
            EnumerableExtensions.AddIfNotNull(failures, TestApiVersion());
            EnumerableExtensions.AddIfNotNull(failures, TestAuthentication());
            EnumerableExtensions.AddIfNotNull(failures, TestCategory());
        }

        private ValidationFailure TestConnection()
        {
            try
            {
                _proxy.GetVersion(Settings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to connect to NZBVortex");

                return new NzbDroneValidationFailure("Host", _localizationService.GetLocalizedString("DownloadClientValidationUnableToConnect", new Dictionary<string, object> { { "clientName", Name } }))
                       {
                           DetailedDescription = ex.Message
                       };
            }

            return null;
        }

        private ValidationFailure TestApiVersion()
        {
            try
            {
                var response = _proxy.GetApiVersion(Settings);
                var version = new Version(response.ApiLevel);

                if (version.Major < 2 || (version.Major == 2 && version.Minor < 3))
                {
                    return new ValidationFailure("Host",
                        _localizationService.GetLocalizedString("DownloadClientValidationErrorVersion",
                            new Dictionary<string, object>
                                { { "clientName", Name }, { "requiredVersion", "2.3" }, { "reportedVersion", version } }));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to connect to NZBVortex");
                return new NzbDroneValidationFailure("Host",  _localizationService.GetLocalizedString("DownloadClientValidationUnableToConnect", new Dictionary<string, object> { { "clientName", Name } }));
            }

            return null;
        }

        private ValidationFailure TestAuthentication()
        {
            try
            {
                _proxy.GetQueue(1, Settings);
            }
            catch (NzbVortexAuthenticationException)
            {
                return new ValidationFailure("ApiKey", _localizationService.GetLocalizedString("DownloadClientValidationApiKeyIncorrect"));
            }

            return null;
        }

        private ValidationFailure TestCategory()
        {
            var group = GetGroups().FirstOrDefault(c => c.GroupName == Settings.TvCategory);

            if (group == null)
            {
                if (StringExtensions.IsNotNullOrWhiteSpace(Settings.TvCategory))
                {
                    return new NzbDroneValidationFailure("TvCategory", _localizationService.GetLocalizedString("DownloadClientValidationGroupMissing"))
                    {
                        DetailedDescription = _localizationService.GetLocalizedString("DownloadClientValidationGroupMissingDetail", new Dictionary<string, object> { { "clientName", Name } })
                    };
                }
            }

            return null;
        }

        private OsPath GetOutputPath(NzbVortexQueueItem vortexQueueItem, DownloadClientItem queueItem)
        {
            var outputPath = _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(vortexQueueItem.DestinationPath));

            if (outputPath.FileName == vortexQueueItem.UiTitle)
            {
                return outputPath;
            }

            // If the release isn't done yet, skip the files check and return null
            if (vortexQueueItem.State != NzbVortexStateType.Done)
            {
                return new OsPath(null);
            }

            var filesResponse = _proxy.GetFiles(vortexQueueItem.Id, Settings);

            if (filesResponse.Count > 1)
            {
                var message = _localizationService.GetLocalizedString("DownloadClientNzbVortexMultipleFilesMessage", new Dictionary<string, object> { { "outputPath", outputPath } });

                queueItem.Status = DownloadItemStatus.Warning;
                queueItem.Message = message;

                _logger.Debug(message);
            }

            return new OsPath(Path.Combine(outputPath.FullPath, filesResponse.First().FileName));
        }
    }
}
