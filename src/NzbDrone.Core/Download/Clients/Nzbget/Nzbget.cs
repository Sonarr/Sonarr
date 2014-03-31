using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using Omu.ValueInjecter;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class Nzbget : DownloadClientBase<NzbgetSettings>, IExecute<TestNzbgetCommand>
    {
        private readonly INzbgetProxy _proxy;
        private readonly IParsingService _parsingService;
        private readonly IHttpProvider _httpProvider;
        private readonly Logger _logger;

        public Nzbget(INzbgetProxy proxy,
                      IParsingService parsingService,
                      IHttpProvider httpProvider,
                      Logger logger)
        {
            _proxy = proxy;
            _parsingService = parsingService;
            _httpProvider = httpProvider;
            _logger = logger;
        }

        public override string DownloadNzb(RemoteEpisode remoteEpisode)
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

        public override IEnumerable<QueueItem> GetQueue()
        {
            List<NzbgetQueueItem> queue;

            try
            {
                queue = _proxy.GetQueue(Settings);
            }
            catch (DownloadClientException ex)
            {
                _logger.ErrorException(ex.Message, ex);
                return Enumerable.Empty<QueueItem>();
            }

            var queueItems = new List<QueueItem>();

            foreach (var item in queue)
            {
                var droneParameter = item.Parameters.SingleOrDefault(p => p.Name == "drone");

                var queueItem = new QueueItem();
                queueItem.Id = droneParameter == null ? item.NzbId.ToString() : droneParameter.Value.ToString();
                queueItem.Title = item.NzbName;
                queueItem.Size = item.FileSizeMb;
                queueItem.Sizeleft = item.RemainingSizeMb;
                queueItem.Status = item.FileSizeMb == item.PausedSizeMb ? "paused" : "queued";

                var parsedEpisodeInfo = Parser.Parser.ParseTitle(queueItem.Title);
                if (parsedEpisodeInfo == null) continue;

                var remoteEpisode = _parsingService.Map(parsedEpisodeInfo, 0);
                if (remoteEpisode.Series == null) continue;

                queueItem.RemoteEpisode = remoteEpisode;
                queueItems.Add(queueItem);
            }

            return queueItems;
        }

        public override IEnumerable<HistoryItem> GetHistory(int start = 0, int limit = 10)
        {
            List<NzbgetHistoryItem> history;

            try
            {
                history = _proxy.GetHistory(Settings);
            }
            catch (DownloadClientException ex)
            {
                _logger.ErrorException(ex.Message, ex);
                return Enumerable.Empty<HistoryItem>();
            }

            var historyItems = new List<HistoryItem>();
            var successStatues = new[] {"SUCCESS", "NONE"};

            foreach (var item in history)
            {
                var droneParameter = item.Parameters.SingleOrDefault(p => p.Name == "drone");
                var status = successStatues.Contains(item.ParStatus) &&
                             successStatues.Contains(item.ScriptStatus)
                    ? HistoryStatus.Completed
                    : HistoryStatus.Failed;

                var historyItem = new HistoryItem();
                historyItem.Id = droneParameter == null ? item.Id.ToString() : droneParameter.Value.ToString();
                historyItem.Title = item.Name;
                historyItem.Size = item.FileSizeMb.ToString(); //Why is this a string?
                historyItem.DownloadTime = 0;
                historyItem.Storage = item.DestDir;
                historyItem.Category = item.Category;
                historyItem.Message = String.Format("PAR Status: {0} - Script Status: {1}", item.ParStatus, item.ScriptStatus);
                historyItem.Status = status;

                historyItems.Add(historyItem);
            }

            return historyItems;
        }

        public override void RemoveFromQueue(string id)
        {
            throw new NotImplementedException();
        }

        public override void RemoveFromHistory(string id)
        {
            _proxy.RemoveFromHistory(id, Settings);
        }

        public override void Test()
        {
            _proxy.GetVersion(Settings);
        }

        private VersionResponse GetVersion(string host = null, int port = 0, string username = null, string password = null)
        {
            return _proxy.GetVersion(Settings);
        }

        public void Execute(TestNzbgetCommand message)
        {
            var settings = new NzbgetSettings();
            settings.InjectFrom(message);

            _proxy.GetVersion(settings);
        }
    }
}