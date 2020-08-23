using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.ClusterAnalysis;
using NzbDrone.Core.Profiles.Delay;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IPrioritizeDownloadDecision
    {
        List<DownloadDecision> PrioritizeDecisions(List<DownloadDecision> decisions);
    }

    public class DownloadDecisionPrioritizationService : IPrioritizeDownloadDecision
    {
        private readonly IConfigService _configService;
        private readonly IDelayProfileService _delayProfileService;

        public DownloadDecisionPrioritizationService(IConfigService configService, IDelayProfileService delayProfileService)
        {
            _configService = configService;
            _delayProfileService = delayProfileService;
        }

        public List<DownloadDecision> PrioritizeDecisions(List<DownloadDecision> decisions)
        {
            return decisions.Where(c => c.RemoteEpisode.Series != null)
                            .GroupBy(c => c.RemoteEpisode.Series.Id, PrioritizeDecisionsWithClusterAnalysis)
                            .SelectMany(c => c)
                            .Union(decisions.Where(c => c.RemoteEpisode.Series == null))
                            .ToList();
        }

        private IEnumerable<DownloadDecision> PrioritizeDecisionsWithClusterAnalysis(int seriesId, IEnumerable<DownloadDecision> downloadDecisions)
        {
            return downloadDecisions
                .Cluster()
                .OrderByDescending(decision => decision, new DownloadDecisionComparer(_configService, _delayProfileService))
                .ThenByClusterDescending(d => d.RemoteEpisode.Release.Size, 200.Megabytes());
        }
    }
}
