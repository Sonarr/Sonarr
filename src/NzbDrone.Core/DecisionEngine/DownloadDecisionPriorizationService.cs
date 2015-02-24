using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

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
                                    var series = downloadDecisions.First().RemoteEpisode.Series;

                                    return downloadDecisions
                                        .OrderByDescending(c => c.RemoteEpisode.ParsedEpisodeInfo.Quality, new QualityModelComparer(series.Profile))
                                        .ThenBy(c => c.RemoteEpisode.Episodes.Select(e => e.EpisodeNumber).MinOrDefault())
                                        .ThenBy(c => PrioritizeDownloadProtocol(series, c.RemoteEpisode.Release.DownloadProtocol))
                                        .ThenByDescending(c => c.RemoteEpisode.Episodes.Count)
                                        .ThenBy(c => c.RemoteEpisode.Release.Size.Round(200.Megabytes()) / Math.Max(1, c.RemoteEpisode.Episodes.Count))
                                        .ThenByDescending(c => TorrentInfo.GetSeeders(c.RemoteEpisode.Release))
                                        .ThenBy(c => c.RemoteEpisode.Release.Age);
                                })
                            .SelectMany(c => c)
                            .Union(decisions.Where(c => c.RemoteEpisode.Series == null))
                            .ToList();
        }

        private int PrioritizeDownloadProtocol(Series series, DownloadProtocol downloadProtocol)
        {
            var delayProfile = _delayProfileService.BestForTags(series.Tags);

            if (downloadProtocol == delayProfile.PreferredProtocol)
            {
                return 0;
            }

            return 1;
        }
    }
}
