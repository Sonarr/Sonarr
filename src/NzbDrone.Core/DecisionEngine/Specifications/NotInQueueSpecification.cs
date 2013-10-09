using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class NotInQueueSpecification : IDecisionEngineSpecification
    {
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly IParsingService _parsingService;
        private readonly Logger _logger;

        public NotInQueueSpecification(IProvideDownloadClient downloadClientProvider, IParsingService parsingService, Logger logger)
        {
            _downloadClientProvider = downloadClientProvider;
            _parsingService = parsingService;
            _logger = logger;
        }

        public string RejectionReason
        {
            get
            {
                return "Already in download queue.";
            }
        }

        public bool IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            var downloadClient = _downloadClientProvider.GetDownloadClient();

            if (!downloadClient.IsConfigured)
            {
                _logger.Warn("Download client {0} isn't configured yet.", downloadClient.GetType().Name);
                return true;
            }

            var queue = downloadClient.GetQueue().Select(queueItem => Parser.Parser.ParseTitle(queueItem.Title)).Where(episodeInfo => episodeInfo != null);

            var mappedQueue = queue.Select(queueItem => _parsingService.Map(queueItem, 0))
                                   .Where(remoteEpisode => remoteEpisode.Series != null);

            return !IsInQueue(subject, mappedQueue);
        }

        public bool IsInQueue(RemoteEpisode newEpisode, IEnumerable<RemoteEpisode> queue)
        {
            var matchingSeries = queue.Where(q => q.Series.Id == newEpisode.Series.Id);
            var matchingTitleWithQuality = matchingSeries.Where(q => q.ParsedEpisodeInfo.Quality >= newEpisode.ParsedEpisodeInfo.Quality);

            return matchingTitleWithQuality.Any(q => q.Episodes.Select(e => e.Id).Intersect(newEpisode.Episodes.Select(e => e.Id)).Any());
        }
    }
}
