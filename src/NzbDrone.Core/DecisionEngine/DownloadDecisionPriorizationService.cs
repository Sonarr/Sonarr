using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Delay;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IPrioritizeDownloadDecision
    {
        List<DownloadDecision> PrioritizeDecisions(List<DownloadDecision> decisions);
    }

    public class DownloadDecisionPriorizationService : IPrioritizeDownloadDecision
    {
        private readonly IDelayProfileService _delayProfileService;

        public DownloadDecisionPriorizationService(IDelayProfileService delayProfileService)
        {
            _delayProfileService = delayProfileService;
        }

        public List<DownloadDecision> PrioritizeDecisions(List<DownloadDecision> decisions)
        {
            return decisions.Where(c => c.RemoteEpisode.Series != null)
                            .GroupBy(c => c.RemoteEpisode.Series.Id, (seriesId, d) =>
                                {
                                    var downloadDecisions = d.ToList();

                                    return downloadDecisions.OrderByDescending(c => ScoreRelease(c.RemoteEpisode));
                                })
                            .SelectMany(c => c)
                            .Union(decisions.Where(c => c.RemoteEpisode.Series == null))
                            .ToList();
        }

        private long ScoreRelease(RemoteEpisode remoteEpisode)
        {
            var score = 0L;

            score += GetQualityScore(remoteEpisode);
            score += GetProtocolScore(remoteEpisode);
            score += GetSizeScore(remoteEpisode);
            score += GetEpisodeCountScore(remoteEpisode);
            score += GetEpisodeNumberScore(remoteEpisode);
            score += GetTorrentScore(remoteEpisode);
            score += GetUsenetScore(remoteEpisode);

            return score;
        }

        private long GetQualityScore(RemoteEpisode remoteEpisode)
        {
            var score = 0;
            score += remoteEpisode.Series.Profile.Value.Items.FindIndex(v => v.Quality == remoteEpisode.ParsedEpisodeInfo.Quality.Quality) * 1000000;
            score += remoteEpisode.ParsedEpisodeInfo.Quality.Revision.Real * 100000;
            score += remoteEpisode.ParsedEpisodeInfo.Quality.Revision.Version * 10000;

            return score;
        }

        private long GetProtocolScore(RemoteEpisode remoteEpisode)
        {
            var delayProfile = _delayProfileService.BestForTags(remoteEpisode.Series.Tags);

            if (remoteEpisode.Release.DownloadProtocol == delayProfile.PreferredProtocol)
            {
                return 5000;
            }

            return 0;
        }

        private long GetSizeScore(RemoteEpisode remoteEpisode)
        {
            var score = remoteEpisode.Release.Size.Round(200.Megabytes()) / Math.Max(1, remoteEpisode.Episodes.Count) / 1024 / 1024 / 100 * -1;

            return score;
        }

        private long GetEpisodeCountScore(RemoteEpisode remoteEpisode)
        {
            if (remoteEpisode.ParsedEpisodeInfo.FullSeason)
            {
                return 100;
            }

            return remoteEpisode.Episodes.Count * -10;
        }

        private long GetEpisodeNumberScore(RemoteEpisode remoteEpisode)
        {
            return remoteEpisode.Episodes.Select(e => e.EpisodeNumber).MinOrDefault() * -1;
        }

        private long GetTorrentScore(RemoteEpisode remoteEpisode)
        {
            var score = 0;
            var seeders = TorrentInfo.GetSeeders(remoteEpisode.Release);
            var peers = TorrentInfo.GetPeers(remoteEpisode.Release);

            if (seeders.HasValue)
            {
                score += Math.Min(seeders.Value, 3000);
            }

            if (peers.HasValue)
            {
                score += Math.Min(peers.Value / 10, 1000);
            }

            return score;
        }

        private long GetUsenetScore(RemoteEpisode remoteEpisode)
        {
            if (remoteEpisode.Release.DownloadProtocol == DownloadProtocol.Usenet)
            {
                if (remoteEpisode.Release.AgeHours < 24)
                {
                    return 1000;
                }

                if (remoteEpisode.Release.Age < 7)
                {
                    return 500;
                }

                return remoteEpisode.Release.Age / -100;
            }

            return 0;
        }
    }
}
