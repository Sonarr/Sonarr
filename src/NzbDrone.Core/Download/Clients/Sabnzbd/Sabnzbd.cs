using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Download.Clients.Sabnzbd.Responses;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using Omu.ValueInjecter;

namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public class Sabnzbd : DownloadClientBase<SabnzbdSettings>, IExecute<TestSabnzbdCommand>
    {
        private readonly IHttpProvider _httpProvider;
        private readonly IParsingService _parsingService;
        private readonly ISabnzbdProxy _sabnzbdProxy;
        private readonly ICached<IEnumerable<QueueItem>> _queueCache;
        private readonly Logger _logger;

        public Sabnzbd(IHttpProvider httpProvider,
                       ICacheManger cacheManger,
                       IParsingService parsingService,
                       ISabnzbdProxy sabnzbdProxy,
                       Logger logger)
        {
            _httpProvider = httpProvider;
            _parsingService = parsingService;
            _sabnzbdProxy = sabnzbdProxy;
            _queueCache = cacheManger.GetCache<IEnumerable<QueueItem>>(GetType(), "queue");
            _logger = logger;
        }

        public override string DownloadNzb(RemoteEpisode remoteEpisode)
        {
            var url = remoteEpisode.Release.DownloadUrl;
            var title = remoteEpisode.Release.Title;
            var category = Settings.TvCategory;
            var priority = remoteEpisode.IsRecentEpisode() ? Settings.RecentTvPriority : Settings.OlderTvPriority;

            using (var nzb = _httpProvider.DownloadStream(url))
            {
                _logger.Info("Adding report [{0}] to the queue.", title);
                var response = _sabnzbdProxy.DownloadNzb(nzb, title, category, priority, Settings);

                if (response != null && response.Ids.Any())
                {
                    return response.Ids.First();
                }

                return null;
            }
        }

        public override IEnumerable<QueueItem> GetQueue()
        {
            return _queueCache.Get("queue", () =>
            {
                var sabQueue = _sabnzbdProxy.GetQueue(0, 0, Settings).Items;

                var queueItems = new List<QueueItem>();

                foreach (var sabQueueItem in sabQueue)
                {
                    var queueItem = new QueueItem();
                    queueItem.Id = sabQueueItem.Id;
                    queueItem.Title = sabQueueItem.Title;
                    queueItem.Size = sabQueueItem.Size;
                    queueItem.Sizeleft = sabQueueItem.Sizeleft;
                    queueItem.Timeleft = sabQueueItem.Timeleft;
                    queueItem.Status = sabQueueItem.Status;

                    var parsedEpisodeInfo = Parser.Parser.ParseTitle(queueItem.Title.Replace("ENCRYPTED / ", ""));
                    if (parsedEpisodeInfo == null) continue;

                    var remoteEpisode = _parsingService.Map(parsedEpisodeInfo, 0);
                    if (remoteEpisode.Series == null) continue;

                    queueItem.RemoteEpisode = remoteEpisode;

                    queueItems.Add(queueItem);
                }

                return queueItems;
            }, TimeSpan.FromSeconds(10));
        }

        public override IEnumerable<HistoryItem> GetHistory(int start = 0, int limit = 10)
        {
            var items = _sabnzbdProxy.GetHistory(start, limit, Settings).Items;
            var historyItems = new List<HistoryItem>();

            foreach (var sabHistoryItem in items)
            {
                var historyItem = new HistoryItem();
                historyItem.Id = sabHistoryItem.Id;
                historyItem.Title = sabHistoryItem.Title;
                historyItem.Size = sabHistoryItem.Size;
                historyItem.DownloadTime = sabHistoryItem.DownloadTime;
                historyItem.Storage = sabHistoryItem.Storage;
                historyItem.Category = sabHistoryItem.Category;
                historyItem.Message = sabHistoryItem.FailMessage;
                historyItem.Status = sabHistoryItem.Status == "Failed" ? HistoryStatus.Failed : HistoryStatus.Completed;

                historyItems.Add(historyItem);
            }

            return historyItems;
        }

        public override void RemoveFromQueue(string id)
        {
            _sabnzbdProxy.RemoveFrom("queue", id, Settings);
        }

        public override void RemoveFromHistory(string id)
        {
            _sabnzbdProxy.RemoveFrom("history", id, Settings);
        }

        public override void Test()
        {
            _sabnzbdProxy.GetCategories(Settings);
        }

        public void Execute(TestSabnzbdCommand message)
        {
            var settings = new SabnzbdSettings();
            settings.InjectFrom(message);

            _sabnzbdProxy.GetCategories(settings);
        }
    }
}