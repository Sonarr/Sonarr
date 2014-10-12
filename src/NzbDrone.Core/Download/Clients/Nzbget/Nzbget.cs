using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;
using NzbDrone.Core.RemotePathMappings;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class Nzbget : UsenetClientBase<NzbgetSettings>
    {
        private readonly INzbgetProxy _proxy;

        public Nzbget(INzbgetProxy proxy,
                      IHttpClient httpClient,
                      IConfigService configService,
                      IDiskProvider diskProvider,
                      IParsingService parsingService,
                      IRemotePathMappingService remotePathMappingService,
                      Logger logger)
            : base(httpClient, configService, diskProvider, remotePathMappingService, logger)
        {
            _proxy = proxy;
        }

        protected override string AddFromNzbFile(RemoteEpisode remoteEpisode, string filename, byte[] fileContent)
        {
            var category = Settings.TvCategory;
            var priority = remoteEpisode.IsRecentEpisode() ? Settings.RecentTvPriority : Settings.OlderTvPriority;

            var response = _proxy.DownloadNzb(fileContent, filename, category, priority, Settings);

            return response;
        }

        private IEnumerable<DownloadClientItem> GetQueue()
        {
            NzbgetGlobalStatus globalStatus;
            List<NzbgetQueueItem> queue;
            Dictionary<Int32, NzbgetPostQueueItem> postQueue;

            try
            {
                globalStatus = _proxy.GetGlobalStatus(Settings);
                queue = _proxy.GetQueue(Settings);
                postQueue = _proxy.GetPostQueue(Settings).ToDictionary(v => v.NzbId);
            }
            catch (DownloadClientException ex)
            {
                _logger.ErrorException(ex.Message, ex);
                return Enumerable.Empty<DownloadClientItem>();
            }

            var queueItems = new List<DownloadClientItem>();

            Int64 totalRemainingSize = 0;

            foreach (var item in queue)
            {
                var postQueueItem = postQueue.GetValueOrDefault(item.NzbId);

                var totalSize = MakeInt64(item.FileSizeHi, item.FileSizeLo);
                var pausedSize = MakeInt64(item.PausedSizeHi, item.PausedSizeLo);
                var remainingSize = MakeInt64(item.RemainingSizeHi, item.RemainingSizeLo);

                var droneParameter = item.Parameters.SingleOrDefault(p => p.Name == "drone");

                var queueItem = new DownloadClientItem();
                queueItem.DownloadClientId = droneParameter == null ? item.NzbId.ToString() : droneParameter.Value.ToString();
                queueItem.Title = item.NzbName;
                queueItem.TotalSize = totalSize;
                queueItem.Category = item.Category;

                if (postQueueItem != null)
                {
                    queueItem.Status = DownloadItemStatus.Downloading;
                    queueItem.Message = postQueueItem.ProgressLabel;

                    if (postQueueItem.StageProgress != 0)
                    {
                        queueItem.RemainingTime = TimeSpan.FromSeconds(postQueueItem.StageTimeSec * 1000 / postQueueItem.StageProgress - postQueueItem.StageTimeSec);
                    }
                }
                else if (globalStatus.DownloadPaused || remainingSize == pausedSize)
                {
                    queueItem.Status = DownloadItemStatus.Paused;
                    queueItem.RemainingSize = remainingSize;
                }
                else
                {
                    if (item.ActiveDownloads == 0 && remainingSize != 0)
                    {
                        queueItem.Status = DownloadItemStatus.Queued;
                    }
                    else
                    {
                        queueItem.Status = DownloadItemStatus.Downloading;
                    }

                    queueItem.RemainingSize = remainingSize - pausedSize;

                    if (globalStatus.DownloadRate != 0)
                    {
                        queueItem.RemainingTime = TimeSpan.FromSeconds((totalRemainingSize + queueItem.RemainingSize) / globalStatus.DownloadRate);
                        totalRemainingSize += queueItem.RemainingSize;
                    }
                }

                queueItems.Add(queueItem);
            }

            return queueItems;
        }

        private IEnumerable<DownloadClientItem> GetHistory()
        {
            List<NzbgetHistoryItem> history;

            try
            {
                history = _proxy.GetHistory(Settings).Take(_configService.DownloadClientHistoryLimit).ToList();
            }
            catch (DownloadClientException ex)
            {
                _logger.ErrorException(ex.Message, ex);
                return Enumerable.Empty<DownloadClientItem>();
            }

            var historyItems = new List<DownloadClientItem>();
            var successStatus = new[] { "SUCCESS", "NONE" };

            foreach (var item in history)
            {
                var droneParameter = item.Parameters.SingleOrDefault(p => p.Name == "drone");

                var historyItem = new DownloadClientItem();
                historyItem.DownloadClient = Definition.Name;
                historyItem.DownloadClientId = droneParameter == null ? item.Id.ToString() : droneParameter.Value.ToString();
                historyItem.Title = item.Name;
                historyItem.TotalSize = MakeInt64(item.FileSizeHi, item.FileSizeLo);
                historyItem.OutputPath = _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(item.DestDir));
                historyItem.Category = item.Category;
                historyItem.Message = String.Format("PAR Status: {0} - Unpack Status: {1} - Move Status: {2} - Script Status: {3} - Delete Status: {4} - Mark Status: {5}", item.ParStatus, item.UnpackStatus, item.MoveStatus, item.ScriptStatus, item.DeleteStatus, item.MarkStatus);
                historyItem.Status = DownloadItemStatus.Completed;
                historyItem.RemainingTime = TimeSpan.Zero;

                if (item.DeleteStatus == "MANUAL")
                {
                    continue;
                }

                if (!successStatus.Contains(item.ParStatus))
                {
                    historyItem.Status = DownloadItemStatus.Failed;
                }

                if (item.UnpackStatus == "SPACE")
                {
                    historyItem.Status = DownloadItemStatus.Warning;
                }
                else if (!successStatus.Contains(item.UnpackStatus))
                {
                    historyItem.Status = DownloadItemStatus.Failed;
                }

                if (!successStatus.Contains(item.MoveStatus))
                {
                    historyItem.Status = DownloadItemStatus.Warning;
                }

                if (!successStatus.Contains(item.ScriptStatus))
                {
                    historyItem.Status = DownloadItemStatus.Failed;
                }

                if (!successStatus.Contains(item.DeleteStatus))
                {
                    historyItem.Status = DownloadItemStatus.Failed;
                }

                historyItems.Add(historyItem);
            }

            return historyItems;
        }

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            MigrateLocalCategoryPath();

            foreach (var downloadClientItem in GetQueue().Concat(GetHistory()))
            {
                if (downloadClientItem.Category == Settings.TvCategory)
                {
                    yield return downloadClientItem;
                }
            }
        }

        public override void RemoveItem(String id)
        {
            _proxy.RemoveFromHistory(id, Settings);
        }

        public override String RetryDownload(String id)
        {
            _proxy.RetryDownload(id, Settings);

            return id;
        }

        public override DownloadClientStatus GetStatus()
        {
            var config = _proxy.GetConfig(Settings);

            var category = GetCategories(config).FirstOrDefault(v => v.Name == Settings.TvCategory);

            var status = new DownloadClientStatus
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost"
            };

            if (category != null)
            {
                status.OutputRootFolders = new List<OsPath> { _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(category.DestDir)) };
            }

            return status;
        }

        protected IEnumerable<NzbgetCategory> GetCategories(Dictionary<String, String> config)
        {
            for (int i = 1; i < 100; i++)
            {
                var name = config.GetValueOrDefault("Category" + i + ".Name");

                if (name == null) yield break;

                var destDir = config.GetValueOrDefault("Category" + i + ".DestDir");

                if (destDir.IsNullOrWhiteSpace())
                {
                    var mainDir = config.GetValueOrDefault("MainDir");
                    destDir = config.GetValueOrDefault("DestDir", String.Empty).Replace("${MainDir}", mainDir);

                    if (config.GetValueOrDefault("AppendCategoryDir", "yes") == "yes")
                    {
                        destDir = Path.Combine(destDir, name);
                    }
                }

                yield return new NzbgetCategory
                {
                    Name = name,
                    DestDir = destDir,
                    Unpack = config.GetValueOrDefault("Category" + i + ".Unpack") == "yes",
                    DefScript = config.GetValueOrDefault("Category" + i + ".DefScript"),
                    Aliases = config.GetValueOrDefault("Category" + i + ".Aliases"),
                };
            }
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
            failures.AddIfNotNull(TestCategory());
        }

        private ValidationFailure TestConnection()
        {
            try
            {
                _proxy.GetVersion(Settings);
            }
            catch (Exception ex)
            {
                if (ex.Message.ContainsIgnoreCase("Authentication failed"))
                {
                    return new ValidationFailure("Username", "Authentication failed");
                }
                _logger.ErrorException(ex.Message, ex);
                return new ValidationFailure("Host", "Unable to connect to NZBGet");
            }

            return null;
        }

        private ValidationFailure TestCategory()
        {
            var config = _proxy.GetConfig(Settings);
            var categories = GetCategories(config);

            if (!Settings.TvCategory.IsNullOrWhiteSpace() && !categories.Any(v => v.Name == Settings.TvCategory))
            {
                return new NzbDroneValidationFailure("TvCategory", "Category does not exist")
                {
                    InfoLink = String.Format("http://{0}:{1}/", Settings.Host, Settings.Port),
                    DetailedDescription = "The Category your entered doesn't exist in NzbGet. Go to NzbGet to create it."
                };
            }

            return null;
        }

        // Javascript doesn't support 64 bit integers natively so json officially doesn't either. 
        // NzbGet api thus sends it in two 32 bit chunks. Here we join the two chunks back together.
        // Simplified decimal example: "42" splits into "4" and "2". To join them I shift (<<) the "4" 1 digit to the left = "40". combine it with "2". which becomes "42" again.
        private Int64 MakeInt64(UInt32 high, UInt32 low)
        {
            Int64 result = high;

            result = (result << 32) | (Int64)low;

            return result;
        }

        // TODO: Remove around January 2015, this code moves the settings to the RemotePathMappingService.
        private void MigrateLocalCategoryPath()
        {
            if (!Settings.TvCategoryLocalPath.IsNullOrWhiteSpace())
            {
                try
                {
                    _logger.Debug("Has legacy TvCategoryLocalPath, trying to migrate to RemotePathMapping list.");

                    var config = _proxy.GetConfig(Settings);
                    var category = GetCategories(config).FirstOrDefault(v => v.Name == Settings.TvCategory);

                    if (category != null)
                    {
                        var localPath = new OsPath(Settings.TvCategoryLocalPath);
                        Settings.TvCategoryLocalPath = null;

                        _remotePathMappingService.MigrateLocalCategoryPath(Definition.Id, Settings, Settings.Host, new OsPath(category.DestDir), localPath);

                        _logger.Info("Discovered Local Category Path for {0}, the setting was automatically moved to the Remote Path Mapping table.", Definition.Name);
                    }
                }
                catch (DownloadClientException ex)
                {
                    _logger.ErrorException("Unable to migrate local category path", ex);
                    throw;
                }
            }
        }
    }
}