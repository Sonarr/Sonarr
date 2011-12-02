using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Indexer;

namespace NzbDrone.Core.Jobs
{
    public class RssSyncJob : IJob
    {
        private readonly IEnumerable<IndexerBase> _indexers;
        private readonly InventoryProvider _inventoryProvider;
        private readonly DownloadProvider _downloadProvider;
        private readonly IndexerProvider _indexerProvider;


        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public RssSyncJob(IEnumerable<IndexerBase> indexers, InventoryProvider inventoryProvider, DownloadProvider downloadProvider, IndexerProvider indexerProvider)
        {
            _indexers = indexers;
            _inventoryProvider = inventoryProvider;
            _downloadProvider = downloadProvider;
            _indexerProvider = indexerProvider;
        }

        public string Name
        {
            get { return "RSS Sync"; }
        }

        public int DefaultInterval
        {
            get { return 15; }
        }

        public void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            var reports = new List<EpisodeParseResult>();

            foreach (var indexer in _indexers.Where(i => _indexerProvider.GetSettings(i.GetType()).Enable))
            {
                try
                {
                    notification.CurrentMessage = "Fetching RSS from " + indexer.Name;
                    reports.AddRange(indexer.FetchRss());
                }
                catch (Exception e)
                {
                    Logger.ErrorException("An error has occurred while fetching items from " + indexer.Name, e);
                }
            }

            Logger.Debug("Finished fetching reports from all indexers. Total {0}", reports.Count);
            notification.CurrentMessage = "Processing downloaded RSS";

            foreach (var episodeParseResult in reports)
            {
                try
                {
                    if (_inventoryProvider.IsMonitored(episodeParseResult) && _inventoryProvider.IsQualityNeeded(episodeParseResult))
                    {
                        _downloadProvider.DownloadReport(episodeParseResult);
                    }
                }
                catch (Exception e)
                {
                    Logger.ErrorException("An error has occurred while processing parse result items from " + episodeParseResult, e);
                }
            }

            notification.CurrentMessage = "RSS Sync Completed";

        }
    }
}