using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.DecisionEngine;

namespace NzbDrone.Core.Jobs
{
    public class RssSyncJob : IJob
    {
        private readonly DownloadProvider _downloadProvider;
        private readonly IndexerService _indexerService;
        private readonly MonitoredEpisodeSpecification _isMonitoredEpisodeSpecification;
        private readonly AllowedDownloadSpecification _allowedDownloadSpecification;
        private readonly UpgradeHistorySpecification _upgradeHistorySpecification;
        private readonly ConfigProvider _configProvider;


        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public RssSyncJob(DownloadProvider downloadProvider, IndexerService indexerService,
            MonitoredEpisodeSpecification isMonitoredEpisodeSpecification, AllowedDownloadSpecification allowedDownloadSpecification, 
            UpgradeHistorySpecification upgradeHistorySpecification, ConfigProvider configProvider)
        {
            _downloadProvider = downloadProvider;
            _indexerService = indexerService;
            _isMonitoredEpisodeSpecification = isMonitoredEpisodeSpecification;
            _allowedDownloadSpecification = allowedDownloadSpecification;
            _upgradeHistorySpecification = upgradeHistorySpecification;
            _configProvider = configProvider;
        }

        public string Name
        {
            get { return "RSS Sync"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromMinutes(_configProvider.RssSyncInterval); }
        }

        public void Start(ProgressNotification notification, dynamic options)
        {
            var reports = new List<EpisodeParseResult>();

            notification.CurrentMessage = "Fetching RSS";

            Parallel.ForEach(_indexerService.GetEnabledIndexers(), indexer =>
            {
                try
                {
                    reports.AddRange(indexer.FetchRss());
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
                    if (_isMonitoredEpisodeSpecification.IsSatisfiedBy(episodeParseResult) &&
                        _allowedDownloadSpecification.IsSatisfiedBy(episodeParseResult) == ReportRejectionType.None &&
                        _upgradeHistorySpecification.IsSatisfiedBy(episodeParseResult))
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