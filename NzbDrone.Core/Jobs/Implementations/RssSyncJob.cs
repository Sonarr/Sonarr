using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;

namespace NzbDrone.Core.Jobs.Implementations
{
    public class RssSyncJob : IJob
    {
        private readonly DownloadProvider _downloadProvider;
        private readonly IIndexerService _indexerService;
        private readonly IDownloadDirector DownloadDirector;
        private readonly IConfigService _configService;


        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public RssSyncJob(DownloadProvider downloadProvider, IIndexerService indexerService, IDownloadDirector downloadDirector, IConfigService configService)
        {
            _downloadProvider = downloadProvider;
            _indexerService = indexerService;
            DownloadDirector = downloadDirector;
            _configService = configService;
        }

        public string Name
        {
            get { return "RSS Sync"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromMinutes(_configService.RssSyncInterval); }
        }

        public void Start(ProgressNotification notification, dynamic options)
        {
            var reports = new List<EpisodeParseResult>();

            notification.CurrentMessage = "Fetching RSS";

            Parallel.ForEach(_indexerService.GetEnabledIndexers(), indexer =>
            {
                try
                {
                    var parseResults = indexer.FetchRss();
                    lock (reports)
                    {
                        reports.AddRange(parseResults);
                    }
                }
                catch (Exception e)
                {
                    Logger.ErrorException("An error has occurred while fetching items from " + indexer.Name, e);
                }
            });

            Logger.Debug("Finished fetching reports from all indexers. Total {0}", reports.Count);

            notification.CurrentMessage = "Processing downloaded RSS";

            foreach (var episodeParseResult in reports)
            {
                try
                {
                    if (DownloadDirector.GetDownloadDecision(episodeParseResult).Approved)
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

            Logger.Info("RSS Sync completed");

        }
    }
}