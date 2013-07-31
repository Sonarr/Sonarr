using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class NotInQueueSpecification : IDecisionEngineSpecification
    {
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly Logger _logger;

        public NotInQueueSpecification(IProvideDownloadClient downloadClientProvider, Logger logger)
        {
            _downloadClientProvider = downloadClientProvider;
            _logger = logger;
        }

        public string RejectionReason
        {
            get
            {
                return "Already in download queue.";
            }
        }

        public bool IsSatisfiedBy(RemoteEpisode subject)
        {
            var downloadClient = _downloadClientProvider.GetDownloadClient();

            if (!downloadClient.IsConfigured)
            {
                _logger.Warn("Download client {0} isn't configured yet.", downloadClient.GetType().Name);
                return true;
            }

            var queue = downloadClient.GetQueue().Select(queueItem => Parser.Parser.ParseTitle(queueItem.Title)).Where(episodeInfo => episodeInfo != null);

            return !IsInQueue(subject, queue);
        }

        private bool IsInQueue(RemoteEpisode newEpisode, IEnumerable<ParsedEpisodeInfo> queue)
        {
            var matchingTitle = queue.Where(q => String.Equals(q.SeriesTitle, newEpisode.Series.CleanTitle, StringComparison.InvariantCultureIgnoreCase));

            var matchingTitleWithQuality = matchingTitle.Where(q => q.Quality >= newEpisode.ParsedEpisodeInfo.Quality);

            if (newEpisode.Series.SeriesType == SeriesTypes.Daily)
            {
                return matchingTitleWithQuality.Any(q => q.AirDate.Value.Date == newEpisode.ParsedEpisodeInfo.AirDate.Value.Date);
            }

            var matchingSeason = matchingTitleWithQuality.Where(q => q.SeasonNumber == newEpisode.ParsedEpisodeInfo.SeasonNumber);

            if (newEpisode.ParsedEpisodeInfo.FullSeason)
            {
                return matchingSeason.Any();
            }

            return matchingSeason.Any(q => q.EpisodeNumbers != null && q.EpisodeNumbers.Any(e => newEpisode.ParsedEpisodeInfo.EpisodeNumbers.Contains(e)));
        }

    }
}
