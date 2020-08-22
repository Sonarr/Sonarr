using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.ClusterAnalysis;
using NzbDrone.Core.Profiles.Delay;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IPrioritizeDownloadDecision
    {
        List<DownloadDecision> PrioritizeDecisions(List<DownloadDecision> decisions);
    }

    public class DownloadDecisionPriorizationService : IPrioritizeDownloadDecision
    {
        private readonly IConfigService _configService;
        private readonly IDelayProfileService _delayProfileService;

        public DownloadDecisionPriorizationService(IConfigService configService, IDelayProfileService delayProfileService)
        {
            _configService = configService;
            _delayProfileService = delayProfileService;
        }

        public List<DownloadDecision> PrioritizeDecisions(List<DownloadDecision> decisions)
        {
            return decisions.Where(c => c.RemoteEpisode.Series != null)
                            .GroupBy(c => c.RemoteEpisode.Series.Id, PrioritizeDecisions)
                            .SelectMany(c => c)
                            .Union(decisions.Where(c => c.RemoteEpisode.Series == null))
                            .ToList();
        }

        private IEnumerable<DownloadDecision> PrioritizeDecisions(int seriesId,
            IEnumerable<DownloadDecision> downloadDecisions)
        {
            if (_configService.UseClusterAnalysis)
            {
                return PrioritizeDecisionsWithClusterAnalysis(seriesId, downloadDecisions);
            }

            return PrioritizeDecisionsWithoutClusterAnalysis(seriesId, downloadDecisions);
        }

        private IEnumerable<DownloadDecision> PrioritizeDecisionsWithClusterAnalysis(int seriesId, IEnumerable<DownloadDecision> downloadDecisions)
        {
            return downloadDecisions
                .Cluster()
                .OrderByDescending(decision => decision, new DownloadDecisionComparer(_configService, _delayProfileService))
                .ThenByClusterDescending(d => d.RemoteEpisode.Release.Size, 200.Megabytes());
        }
        private IEnumerable<DownloadDecision> PrioritizeDecisionsWithoutClusterAnalysis(int seriesId, IEnumerable<DownloadDecision> downloadDecisions)
        {
            return downloadDecisions
                .OrderByDescending(decision => decision,
                    new DownloadDecisionComparer(_configService, _delayProfileService));
        }
    }
}
