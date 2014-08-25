using System.Linq;
using NLog;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine.Specifications.RssSync
{
    public class DelaySpecification : IDecisionEngineSpecification
    {
        private readonly IPendingReleaseService _pendingReleaseService;
        private readonly Logger _logger;

        public DelaySpecification(IPendingReleaseService pendingReleaseService, Logger logger)
        {
            _pendingReleaseService = pendingReleaseService;
            _logger = logger;
        }

        public string RejectionReason
        {
            get
            {
                return "Waiting for better quality release";
            }
        }

        public RejectionType Type { get { return RejectionType.Temporary; } }

        public virtual bool IsSatisfiedBy(RemoteEpisode subject, SearchCriteriaBase searchCriteria)
        {
            //How do we want to handle drone being off and the automatic search being triggered?
            //TODO: Add a flag to the search to state it is a "scheduled" search

            if (searchCriteria != null)
            {
                _logger.Debug("Ignore delay for searches");
                return true;
            }

            var profile = subject.Series.Profile.Value;

            if (profile.GrabDelay == 0)
            {
                _logger.Debug("Profile does not delay before download");
                return true;
            }

            var comparer = new QualityModelComparer(profile);

            if (subject.ParsedEpisodeInfo.Quality.Proper)
            {
                foreach (var file in subject.Episodes.Where(c => c.EpisodeFileId != 0).Select(c => c.EpisodeFile.Value))
                {
                    if (comparer.Compare(subject.ParsedEpisodeInfo.Quality, file.Quality) > 0)
                    {
                        var properCompare = subject.ParsedEpisodeInfo.Quality.Proper.CompareTo(file.Quality.Proper);

                        if (subject.ParsedEpisodeInfo.Quality.Quality == file.Quality.Quality && properCompare > 0)
                        {
                            _logger.Debug("New quality is a proper for existing quality, skipping delay");
                            return true;
                        }
                    }
                }
            }

            //If quality meets or exceeds the best allowed quality in the profile accept it immediately
            var bestQualityInProfile = new QualityModel(profile.Items.Last(q => q.Allowed).Quality);
            var bestCompare = comparer.Compare(subject.ParsedEpisodeInfo.Quality, bestQualityInProfile);

            if (bestCompare >= 0)
            {
                _logger.Debug("Quality is highest in profile, will not delay");
                return true;
            }

            if (profile.GrabDelayMode == GrabDelayMode.Cutoff)
            {
                var cutoff = new QualityModel(profile.Cutoff);
                var cutoffCompare = comparer.Compare(subject.ParsedEpisodeInfo.Quality, cutoff);

                if (cutoffCompare >= 0)
                {
                    _logger.Debug("Quality meets or exceeds the cutoff, will not delay");
                    return true;
                }
            }

            if (profile.GrabDelayMode == GrabDelayMode.First)
            {
                var episodeIds = subject.Episodes.Select(e => e.Id);

                var oldest = _pendingReleaseService.GetPendingRemoteEpisodes(subject.Series.Id)
                                                            .Where(r => r.Episodes.Select(e => e.Id).Intersect(episodeIds).Any())
                                                            .OrderByDescending(p => p.Release.AgeHours)
                                                            .FirstOrDefault();

                if (oldest != null && oldest.Release.AgeHours > profile.GrabDelay)
                {
                    return true;
                }
            }

            if (subject.Release.AgeHours < profile.GrabDelay)
            {
                _logger.Debug("Age ({0}) is less than delay {1}, delaying", subject.Release.AgeHours, profile.GrabDelay);
                return false;
            }

            return true;
        }
    }
}
