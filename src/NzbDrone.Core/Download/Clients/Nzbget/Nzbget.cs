using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using Omu.ValueInjecter;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class Nzbget : DownloadClientBase<NzbgetSettings>, IExecute<TestNzbgetCommand>
    {
        private readonly INzbgetProxy _proxy;
        private readonly IHttpProvider _httpProvider;

        public Nzbget(INzbgetProxy proxy,
                      IConfigService configService,
                      IParsingService parsingService,
                      IHttpProvider httpProvider,
                      Logger logger)
            : base(configService, parsingService, logger)
        {
            _proxy = proxy;
            _httpProvider = httpProvider;
        }

        public override DownloadProtocol Protocol
        {
            get
            {
                return DownloadProtocol.Usenet;
            }
        }

        public override string Download(RemoteEpisode remoteEpisode)
        {
            var url = remoteEpisode.Release.DownloadUrl;
            var title = remoteEpisode.Release.Title + ".nzb";

            string category = Settings.TvCategory;
            int priority = remoteEpisode.IsRecentEpisode() ? Settings.RecentTvPriority : Settings.OlderTvPriority;

            _logger.Info("Adding report [{0}] to the queue.", title);

            using (var nzb = _httpProvider.DownloadStream(url))
            {
                _logger.Info("Adding report [{0}] to the queue.", title);
                var response = _proxy.DownloadNzb(nzb, title, category, priority, Settings);

                return response;
            }
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
            var successStatus = new[] {"SUCCESS", "NONE"};

            foreach (var item in history)
            {
                var droneParameter = item.Parameters.SingleOrDefault(p => p.Name == "drone");

                var historyItem = new DownloadClientItem();
                historyItem.DownloadClient = Definition.Name;
                historyItem.DownloadClientId = droneParameter == null ? item.Id.ToString() : droneParameter.Value.ToString();
                historyItem.Title = item.Name;
                historyItem.TotalSize = MakeInt64(item.FileSizeHi, item.FileSizeLo);
                historyItem.OutputPath = item.DestDir;
                historyItem.Category = item.Category;
                historyItem.Message = String.Format("PAR Status: {0} - Unpack Status: {1} - Move Status: {2} - Script Status: {3} - Delete Status: {4} - Mark Status: {5}", item.ParStatus, item.UnpackStatus, item.MoveStatus, item.ScriptStatus, item.DeleteStatus, item.MarkStatus);
                historyItem.Status = DownloadItemStatus.Completed;
                historyItem.RemainingTime = TimeSpan.Zero;

                if (item.DeleteStatus == "MANUAL")
                {
                    continue;
                }

                if (!successStatus.Contains(item.ParStatus) ||
                         !successStatus.Contains(item.UnpackStatus) ||
                         !successStatus.Contains(item.MoveStatus) ||
                         !successStatus.Contains(item.ScriptStatus))
                {
                    historyItem.Status = DownloadItemStatus.Failed;
                }

                historyItems.Add(historyItem);
            }

            return historyItems;
        }

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            foreach (var downloadClientItem in GetQueue().Concat(GetHistory()))
            {
                if (downloadClientItem.Category != Settings.TvCategory) continue;

                downloadClientItem.RemoteEpisode = GetRemoteEpisode(downloadClientItem.Title);
                if (downloadClientItem.RemoteEpisode == null) continue;

                yield return downloadClientItem;
            }
        }

        public override void RemoveItem(string id)
        {
            _proxy.RemoveFromHistory(id, Settings);
        }

        public override void RetryDownload(string id)
        {
            _proxy.RetryDownload(id, Settings);
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
                status.OutputRootFolders = new List<string> { category.DestDir };
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

        private String GetVersion(string host = null, int port = 0, string username = null, string password = null)
        {
            return _proxy.GetVersion(Settings);
        }

        public override void Test(NzbgetSettings settings)
        {
            _proxy.GetVersion(settings);

            var config = _proxy.GetConfig(settings);

            var categories = GetCategories(config);

            if (!categories.Any(v => v.Name == settings.TvCategory))
            {
                throw new ApplicationException("Category does not exist");
            }
        }

        public void Execute(TestNzbgetCommand message)
        {
            var settings = new NzbgetSettings();
            settings.InjectFrom(message);

            Test(settings);
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
    }
}