using System;
using System.Collections.Generic;
using NLog;
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
        private readonly Logger _logger;

        public Nzbget(INzbgetProxy proxy,
                      IParsingService parsingService,
                      Logger logger)
        {
            _proxy = proxy;
            _parsingService = parsingService;
            _logger = logger;
        }

        public override string DownloadNzb(RemoteEpisode remoteEpisode)
        {
            var url = remoteEpisode.Release.DownloadUrl;
            var title = remoteEpisode.Release.Title + ".nzb";

            string cat = Settings.TvCategory;
            int priority = remoteEpisode.IsRecentEpisode() ? Settings.RecentTvPriority : Settings.OlderTvPriority;

            _logger.Info("Adding report [{0}] to the queue.", title);

            var success = _proxy.AddNzb(Settings, title, cat, priority, false, url);

            _logger.Debug("Queue Response: [{0}]", success);

            return null;
        }

        public override IEnumerable<QueueItem> GetQueue()
        {
            var items = _proxy.GetQueue(Settings);

            foreach (var nzbGetQueueItem in items)
            {
                var queueItem = new QueueItem();
                queueItem.Id = nzbGetQueueItem.NzbId.ToString();
                queueItem.Title = nzbGetQueueItem.NzbName;
                queueItem.Size = nzbGetQueueItem.FileSizeMb;
                queueItem.Sizeleft = nzbGetQueueItem.RemainingSizeMb;
                queueItem.Status = nzbGetQueueItem.FileSizeMb == nzbGetQueueItem.PausedSizeMb ? "paused" : "queued";

                var parsedEpisodeInfo = Parser.Parser.ParseTitle(queueItem.Title);
                if (parsedEpisodeInfo == null) continue;

                var remoteEpisode = _parsingService.Map(parsedEpisodeInfo, 0);
                if (remoteEpisode.Series == null) continue;

                queueItem.RemoteEpisode = remoteEpisode;

                yield return queueItem;
            }
        }

        public override IEnumerable<HistoryItem> GetHistory(int start = 0, int limit = 10)
        {
            return new HistoryItem[0];
        }

        public override void RemoveFromQueue(string id)
        {
            throw new NotImplementedException();
        }

        public override void RemoveFromHistory(string id)
        {
            throw new NotImplementedException();
        }

        public VersionResponse GetVersion(string host = null, int port = 0, string username = null, string password = null)
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