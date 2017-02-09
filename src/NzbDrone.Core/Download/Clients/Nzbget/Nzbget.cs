using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
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

            var addpaused = Settings.AddPaused;

            var response = _proxy.DownloadNzb(fileContent, filename, category, priority, addpaused, Settings);

            if (response == null)
            {
                throw new DownloadClientException("Failed to add nzb {0}", filename);
            }

            return response;
        }

        private IEnumerable<DownloadClientItem> GetQueue()
        {
            NzbgetGlobalStatus globalStatus;
            List<NzbgetQueueItem> queue;

            try
            {
                globalStatus = _proxy.GetGlobalStatus(Settings);
                queue = _proxy.GetQueue(Settings);
            }
            catch (DownloadClientException ex)
            {
                _logger.Error(ex);
                return Enumerable.Empty<DownloadClientItem>();
            }

            var queueItems = new List<DownloadClientItem>();

            long totalRemainingSize = 0;

            foreach (var item in queue)
            {
                var totalSize = MakeInt64(item.FileSizeHi, item.FileSizeLo);
                var pausedSize = MakeInt64(item.PausedSizeHi, item.PausedSizeLo);
                var remainingSize = MakeInt64(item.RemainingSizeHi, item.RemainingSizeLo);

                var droneParameter = item.Parameters.SingleOrDefault(p => p.Name == "drone");

                var queueItem = new DownloadClientItem();
                queueItem.DownloadId = droneParameter == null ? item.NzbId.ToString() : droneParameter.Value.ToString();
                queueItem.Title = item.NzbName;
                queueItem.TotalSize = totalSize;
                queueItem.Category = item.Category;
                queueItem.DownloadClient = Definition.Name;

                if (globalStatus.DownloadPaused || remainingSize == pausedSize && remainingSize != 0)
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
                _logger.Error(ex);
                return Enumerable.Empty<DownloadClientItem>();
            }

            var historyItems = new List<DownloadClientItem>();
            var successStatus = new[] { "SUCCESS", "NONE" };

            foreach (var item in history)
            {
                var droneParameter = item.Parameters.SingleOrDefault(p => p.Name == "drone");

                var historyItem = new DownloadClientItem();
                historyItem.DownloadClient = Definition.Name;
                historyItem.DownloadId = droneParameter == null ? item.Id.ToString() : droneParameter.Value.ToString();
                historyItem.Title = item.Name;
                historyItem.TotalSize = MakeInt64(item.FileSizeHi, item.FileSizeLo);
                historyItem.OutputPath = _remotePathMappingService.RemapRemoteToLocal(Settings.Host, new OsPath(item.DestDir));
                historyItem.Category = item.Category;
                historyItem.Message = string.Format("PAR Status: {0} - Unpack Status: {1} - Move Status: {2} - Script Status: {3} - Delete Status: {4} - Mark Status: {5}", item.ParStatus, item.UnpackStatus, item.MoveStatus, item.ScriptStatus, item.DeleteStatus, item.MarkStatus);
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

                if (!successStatus.Contains(item.DeleteStatus) && item.DeleteStatus.IsNotNullOrWhiteSpace())
                {
                    if (item.DeleteStatus == "COPY" || item.DeleteStatus == "DUPE")
                    {
                        historyItem.Status = DownloadItemStatus.Failed;
                    }
                    else
                    {
                        historyItem.Status = DownloadItemStatus.Warning;
                    }
                }

                if (item.DeleteStatus == "HEALTH")
                {
                    historyItem.Status = DownloadItemStatus.Failed;
                }

                historyItems.Add(historyItem);
            }

            return historyItems;
        }

        public override string Name => "NZBGet";

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            return GetQueue().Concat(GetHistory()).Where(downloadClientItem => downloadClientItem.Category == Settings.TvCategory);
        }

        public override void RemoveItem(string downloadId, bool deleteData)
        {
            if (deleteData)
            {
                DeleteItemData(downloadId);
            }

            _proxy.RemoveItem(downloadId, Settings);
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

        protected IEnumerable<NzbgetCategory> GetCategories(Dictionary<string, string> config)
        {
            for (int i = 1; i < 100; i++)
            {
                var name = config.GetValueOrDefault("Category" + i + ".Name");

                if (name == null) yield break;

                var destDir = config.GetValueOrDefault("Category" + i + ".DestDir");

                if (destDir.IsNullOrWhiteSpace())
                {
                    var mainDir = config.GetValueOrDefault("MainDir");
                    destDir = config.GetValueOrDefault("DestDir", string.Empty).Replace("${MainDir}", mainDir);

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
            failures.AddIfNotNull(TestSettings());
        }

        private ValidationFailure TestConnection()
        {
            try
            {
                var version = _proxy.GetVersion(Settings).Split('-')[0];

                if (Version.Parse(version) < Version.Parse("12.0"))
                {
                    return new ValidationFailure(string.Empty, "Nzbget version too low, need 12.0 or higher");
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.ContainsIgnoreCase("Authentication failed"))
                {
                    return new ValidationFailure("Username", "Authentication failed");
                }
                _logger.Error(ex);
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
                    InfoLink = string.Format("http://{0}:{1}/", Settings.Host, Settings.Port),
                    DetailedDescription = "The Category your entered doesn't exist in NzbGet. Go to NzbGet to create it."
                };
            }

            return null;
        }

        private ValidationFailure TestSettings()
        {
            var config = _proxy.GetConfig(Settings);

            var keepHistory = config.GetValueOrDefault("KeepHistory");
            if (keepHistory == "0")
            {
                return new NzbDroneValidationFailure(string.Empty, "NzbGet setting KeepHistory should be greater than 0")
                {
                    InfoLink = string.Format("http://{0}:{1}/", Settings.Host, Settings.Port),
                    DetailedDescription = "NzbGet setting KeepHistory is set to 0. Which prevents Sonarr from seeing completed downloads."
                };
            }

            return null;
        }

        // Javascript doesn't support 64 bit integers natively so json officially doesn't either.
        // NzbGet api thus sends it in two 32 bit chunks. Here we join the two chunks back together.
        // Simplified decimal example: "42" splits into "4" and "2". To join them I shift (<<) the "4" 1 digit to the left = "40". combine it with "2". which becomes "42" again.
        private long MakeInt64(uint high, uint low)
        {
            long result = high;

            result = (result << 32) | (long)low;

            return result;
        }
    }
}
