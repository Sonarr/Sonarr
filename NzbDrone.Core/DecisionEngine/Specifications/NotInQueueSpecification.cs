using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class NotInQueueSpecification : IDecisionEngineSpecification
    {
        private readonly IProvideDownloadClient _downloadClientProvider;

        public NotInQueueSpecification(IProvideDownloadClient downloadClientProvider)
        {
            _downloadClientProvider = downloadClientProvider;
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
