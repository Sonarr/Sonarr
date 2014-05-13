using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IPrioritizeDownloadDecision
    {
        List<DownloadDecision> PrioritizeDecisions(List<DownloadDecision> decisions);
    }

    public class DownloadDecisionPriorizationService : IPrioritizeDownloadDecision
    {
        public List<DownloadDecision> PrioritizeDecisions(List<DownloadDecision> decisions)
        {
            return decisions
                .Where(c => c.RemoteEpisode.Series != null)
                .GroupBy(c => c.RemoteEpisode.Series.Id, (i, s) => s
                    .OrderByDescending(c => c.RemoteEpisode.ParsedEpisodeInfo.Quality, new QualityModelComparer(s.First().RemoteEpisode.Series.Profile))
                    .ThenBy(c => c.RemoteEpisode.Episodes.Select(e => e.EpisodeNumber).MinOrDefault())
                    .ThenBy(c => c.RemoteEpisode.Release.DownloadProtocol)
                    .ThenBy(c => c.RemoteEpisode.Release.Size.Round(200.Megabytes()) / Math.Max(1, c.RemoteEpisode.Episodes.Count))
                    .ThenByDescending(c => TorrentInfo.GetSeeders(c.RemoteEpisode.Release))
                    .ThenBy(c => c.RemoteEpisode.Release.Age))
                .SelectMany(c => c)
                .Union(decisions.Where(c => c.RemoteEpisode.Series == null))
                .ToList();
        }
    }
}
