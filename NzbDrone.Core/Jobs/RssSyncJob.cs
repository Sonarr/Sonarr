using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ninject;
using NLog;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.DecisionEngine;
using NzbDrone.Core.Providers.Indexer;
using StackExchange.Profiling;

namespace NzbDrone.Core.Jobs
{
    public class RssSyncJob : IJob
    {
        private readonly DownloadProvider _downloadProvider;
        private readonly IndexerProvider _indexerProvider;
        private readonly MonitoredEpisodeSpecification _isMonitoredEpisodeSpecification;
        private readonly AllowedDownloadSpecification _allowedDownloadSpecification;
        private readonly UpgradeHistorySpecification _upgradeHistorySpecification;


        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public RssSyncJob(DownloadProvider downloadProvider, IndexerProvider indexerProvider,
            MonitoredEpisodeSpecification isMonitoredEpisodeSpecification, AllowedDownloadSpecification allowedDownloadSpecification, UpgradeHistorySpecification upgradeHistorySpecification)
        {
            _downloadProvider = downloadProvider;
            _indexerProvider = indexerProvider;
            _isMonitoredEpisodeSpecification = isMonitoredEpisodeSpecification;
            _allowedDownloadSpecification = allowedDownloadSpecification;
            _upgradeHistorySpecification = upgradeHistorySpecification;
        }

        public string Name
        {
            get { return "RSS Sync"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromMinutes(25); }
        }

        public void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            var reports = new List<EpisodeParseResult>();

            notification.CurrentMessage = "Fetching RSS";

            Parallel.ForEach(_indexerProvider.GetEnabledIndexers(), indexer =>
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