using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.Indexer;

namespace NzbDrone.Core.Providers.Jobs
{
    public class RssSyncJob : IJob
    {
        private readonly IEnumerable<IndexerBase> _indexers;
        private readonly InventoryProvider _inventoryProvider;
        private readonly DownloadProvider _downloadProvider;


        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public RssSyncJob(IEnumerable<IndexerBase> indexers, InventoryProvider inventoryProvider, DownloadProvider downloadProvider)
        {
            _indexers = indexers;
            _inventoryProvider = inventoryProvider;
            _downloadProvider = downloadProvider;
        }

        public string Name
        {
            get { return "RSS Sync"; }
        }

        public int DefaultInterval
        {
            get { return 15; }
        }

        public void Start(ProgressNotification notification, int targetId)
        {
            var reports = new List<EpisodeParseResult>();

            foreach (var indexer in _indexers.Where(i => i.Settings.Enable))
            {
                try
                {
                    notification.CurrentMessage = "Fetching RSS from " + indexer.Name;
                    reports.AddRange(indexer.Fetch());
                }
                catch (Exception e)
                {
                    Logger.ErrorException("An error has occured while fetching items from " + indexer.Name, e);
                }
            }

            Logger.Debug("Finished fetching reports from all indexers. Total {0}", reports.Count);
            notification.CurrentMessage = "Proccessing downloaded RSS";

            foreach (var episodeParseResult in reports)
            {
                try
                {
                    if (_inventoryProvider.IsNeeded(episodeParseResult))
                    {
                        _downloadProvider.DownloadReport(episodeParseResult);
                    }
                }
                catch (Exception e)
                {
                    Logger.ErrorException("An error has occured while processing parse result items from " + episodeParseResult, e);
                }
            }

        }
    }
}