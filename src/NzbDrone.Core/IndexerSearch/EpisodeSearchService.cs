using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Download;
using NzbDrone.Core.Instrumentation.Extensions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Queue;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.IndexerSearch
{
    public interface IEpisodeSearchService
    {
        void MissingEpisodesAiredAfter(DateTime dateTime, IEnumerable<Int32> grabbed
            );
    }

    public class MissingEpisodeSearchService : IEpisodeSearchService, IExecute<EpisodeSearchCommand>, IExecute<MissingEpisodeSearchCommand>
    {
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IDownloadApprovedReports _downloadApprovedReports;
        private readonly IEpisodeService _episodeService;
        private readonly IQueueService _queueService;
        private readonly Logger _logger;

        public MissingEpisodeSearchService(ISearchForNzb nzbSearchService,
                                    IDownloadApprovedReports downloadApprovedReports,
                                    IEpisodeService episodeService,
                                    IQueueService queueService,
                                    Logger logger)
        {
            _nzbSearchService = nzbSearchService;
            _downloadApprovedReports = downloadApprovedReports;
            _episodeService = episodeService;
            _queueService = queueService;
            _logger = logger;
        }

        public void MissingEpisodesAiredAfter(DateTime dateTime, IEnumerable<Int32> grabbed)
        {
            var missing = _episodeService.EpisodesBetweenDates(dateTime, DateTime.UtcNow)
                                         .Where(e => !e.HasFile &&
                                                !_queueService.GetQueue().Select(q => q.Episode.Id).Contains(e.Id) &&
                                                !grabbed.Contains(e.Id))
                                         .ToList();

            var downloadedCount = 0;
            _logger.Info("Searching for {0} missing episodes since last RSS Sync", missing.Count);

            foreach (var episode in missing)
            {
                var decisions = _nzbSearchService.EpisodeSearch(episode);
                var downloaded = _downloadApprovedReports.DownloadApproved(decisions);
                downloadedCount += downloaded.Count;
            }

            _logger.ProgressInfo("Completed search for {0} episodes. {1} reports downloaded.", missing.Count, downloadedCount);
        }

        public void Execute(EpisodeSearchCommand message)
        {
            foreach (var episodeId in message.EpisodeIds)
            {
                var decisions = _nzbSearchService.EpisodeSearch(episodeId);
                var downloaded = _downloadApprovedReports.DownloadApproved(decisions);

                _logger.ProgressInfo("Episode search completed. {0} reports downloaded.", downloaded.Count);
            }
        }

        public void Execute(MissingEpisodeSearchCommand message)
        {
            //TODO: Look at ways to make this more efficient (grouping by series/season)

            var episodes =
                _episodeService.EpisodesWithoutFiles(new PagingSpec<Episode>
                                                     {
                                                         Page = 1,
                                                         PageSize = 100000,
                                                         SortDirection = SortDirection.Ascending,
                                                         SortKey = "Id",
                                                         FilterExpression = v => v.Monitored == true && v.Series.Monitored == true
                                                     }).Records.ToList();

            var missing = episodes.Where(e => !_queueService.GetQueue().Select(q => q.Episode.Id).Contains(e.Id)).ToList();

            _logger.ProgressInfo("Performing missing search for {0} episodes", missing.Count);
            var downloadedCount = 0;

            //Limit requests to indexers at 100 per minute
            using (var rateGate = new RateGate(100, TimeSpan.FromSeconds(60)))
            {
                foreach (var episode in missing)
                {
                    rateGate.WaitToProceed();
                    var decisions = _nzbSearchService.EpisodeSearch(episode);
                    var downloaded = _downloadApprovedReports.DownloadApproved(decisions);
                    downloadedCount += downloaded.Count;
                }
            }

            _logger.ProgressInfo("Completed missing search for {0} episodes. {1} reports downloaded.", missing.Count, downloadedCount);
        }
    }
}
