using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.Model;
using NzbDrone.Core.Parser;
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

        public virtual bool IsSatisfiedBy(RemoteEpisode subject)
        {
            var downloadClient = _downloadClientProvider.GetDownloadClient();

            var queue = downloadClient.GetQueue().Select(q => Parser.Parser.ParseTitle(q.Title));

            return !IsInQueue(subject, queue);
        }

        public virtual bool IsInQueue(RemoteEpisode newEpisode, IEnumerable<ParsedEpisodeInfo> queue)
        {
            var matchingTitle = queue.Where(q => String.Equals(q.SeriesTitle, newEpisode.Series.CleanTitle, StringComparison.InvariantCultureIgnoreCase));

            var matchingTitleWithQuality = matchingTitle.Where(q => q.Quality >= newEpisode.Quality);

            if (newEpisode.Series.SeriesType == SeriesTypes.Daily)
            {
                return matchingTitleWithQuality.Any(q => q.AirDate.Value.Date == newEpisode.AirDate.Value.Date);
            }

            var matchingSeason = matchingTitleWithQuality.Where(q => q.SeasonNumber == newEpisode.SeasonNumber);

            if (newEpisode.FullSeason)
            {
                return matchingSeason.Any();
            }

            return matchingSeason.Any(q => q.EpisodeNumbers != null && q.EpisodeNumbers.Any(e => newEpisode.EpisodeNumbers.Contains(e)));
        }

    }
}
