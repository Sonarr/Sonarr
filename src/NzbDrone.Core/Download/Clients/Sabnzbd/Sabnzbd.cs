using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using Omu.ValueInjecter;

namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public class Sabnzbd : DownloadClientBase<SabnzbdSettings>
    {
        private readonly IHttpProvider _httpProvider;
        private readonly ISabnzbdProxy _proxy;

        public Sabnzbd(IHttpProvider httpProvider,
                       ISabnzbdProxy proxy,
                       IConfigService configService,
                       IParsingService parsingService,
                       Logger logger)
            : base(configService, parsingService, logger)
        {
            _httpProvider = httpProvider;
            _proxy = proxy;
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
            var title = remoteEpisode.Release.Title;
            var category = Settings.TvCategory;
            var priority = remoteEpisode.IsRecentEpisode() ? Settings.RecentTvPriority : Settings.OlderTvPriority;

            using (var nzb = _httpProvider.DownloadStream(url))
            {
                _logger.Info("Adding report [{0}] to the queue.", title);
                var response = _proxy.DownloadNzb(nzb, title, category, priority, Settings);

                if (response != null && response.Ids.Any())
                {
                    return response.Ids.First();
                }

                return null;
            }
        }

        private IEnumerable<DownloadClientItem> GetQueue()
        {
            SabnzbdQueue sabQueue;

            try
            {
                sabQueue = _proxy.GetQueue(0, 0, Settings);
            }
            catch (DownloadClientException ex)
            {
                _logger.ErrorException(ex.Message, ex);
                return Enumerable.Empty<DownloadClientItem>();
            }

            var queueItems = new List<DownloadClientItem>();

            foreach (var sabQueueItem in sabQueue.Items)
            {
                var queueItem = new DownloadClientItem();
                queueItem.DownloadClient = Definition.Name;
                queueItem.DownloadClientId = sabQueueItem.Id;
                queueItem.Category = sabQueueItem.Category;
                queueItem.Title = sabQueueItem.Title;
                queueItem.TotalSize = (long)(sabQueueItem.Size * 1024 * 1024);
                queueItem.RemainingSize = (long)(sabQueueItem.Sizeleft * 1024 * 1024);
                queueItem.RemainingTime = sabQueueItem.Timeleft;

                if (sabQueue.Paused || sabQueueItem.Status == SabnzbdDownloadStatus.Paused)
                {
                    queueItem.Status = DownloadItemStatus.Paused;

                    queueItem.RemainingTime = null;
                }
                else if (sabQueueItem.Status == SabnzbdDownloadStatus.Queued || sabQueueItem.Status == SabnzbdDownloadStatus.Grabbing)
                {
                    queueItem.Status = DownloadItemStatus.Queued;
                }
                else
                {
                    queueItem.Status = DownloadItemStatus.Downloading;
                }

                if (queueItem.Title.StartsWith("ENCRYPTED /"))
                {
                    queueItem.Title = queueItem.Title.Substring(11);
                    queueItem.IsEncrypted = true;
                }

                queueItems.Add(queueItem);
            }

            return queueItems;
        }

        private IEnumerable<DownloadClientItem> GetHistory()
        {
            SabnzbdHistory sabHistory;

            try
            {
                sabHistory = _proxy.GetHistory(0, _configService.DownloadClientHistoryLimit, Settings);
            }
            catch (DownloadClientException ex)
            {
                _logger.ErrorException(ex.Message, ex);
                return Enumerable.Empty<DownloadClientItem>();
            }

            var historyItems = new List<DownloadClientItem>();

            foreach (var sabHistoryItem in sabHistory.Items)
            {
                var historyItem = new DownloadClientItem
                {
                    DownloadClient = Definition.Name,
                    DownloadClientId = sabHistoryItem.Id,
                    Category = sabHistoryItem.Category,
                    Title = sabHistoryItem.Title,

                    TotalSize = sabHistoryItem.Size,
                    RemainingSize = 0,
                    DownloadTime = TimeSpan.FromSeconds(sabHistoryItem.DownloadTime),
                    RemainingTime = TimeSpan.Zero,

                    Message = sabHistoryItem.FailMessage
                };

                if (sabHistoryItem.Status == SabnzbdDownloadStatus.Failed)
                {
                    historyItem.Status = DownloadItemStatus.Failed;
                }
                else if (sabHistoryItem.Status == SabnzbdDownloadStatus.Completed)
                {
                    historyItem.Status = DownloadItemStatus.Completed;
                }
                else // Verifying/Moving etc
                {
                    historyItem.Status = DownloadItemStatus.Downloading;
                }

                if (!sabHistoryItem.Storage.IsNullOrWhiteSpace())
                {
                    var parent = sabHistoryItem.Storage.GetParentPath();
                    if (parent != null && Path.GetFileName(parent) == sabHistoryItem.Title)
                    {
                        historyItem.OutputPath = parent;
                    }
                    else
                    {
                        historyItem.OutputPath = sabHistoryItem.Storage;
                    }
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
            if (GetQueue().Any(v => v.DownloadClientId == id))
            {
                _proxy.RemoveFrom("queue", id, Settings);
            }
            else
            {
                _proxy.RemoveFrom("history", id, Settings);
            }
        }

        public override void RetryDownload(string id)
        {
            _proxy.RetryDownload(id, Settings);
        }

        public override DownloadClientStatus GetStatus()
        {
            var status = new DownloadClientStatus
            {
                IsLocalhost = Settings.Host == "127.0.0.1" || Settings.Host == "localhost"
            };

            return status;
        }

        public override IEnumerable<ValidationFailure> Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(ConnectionTest());
            failures.AddIfNotNull(ValidateCategory());

            return failures;
        }

        private ValidationFailure ConnectionTest()
        {
            try
            {
                _proxy.GetCategories(Settings);
            }
            catch (Exception ex)
            {
                _logger.ErrorException(ex.Message, ex);
                return new ValidationFailure("Host", "Unable to connect to SABnzbd");
            }

            return null;
        }

        private ValidationFailure ValidateCategory()
        {
            var categories = _proxy.GetCategories(Settings);

            if (!Settings.TvCategory.IsNullOrWhiteSpace() && !categories.Any(v => v == Settings.TvCategory))
            {
                return new ValidationFailure("TvCategory", "Category does not exist");
            }

            return null;
        }
    }
}