using System.Collections.Generic;
using NzbDrone.Core.DecisionEngine;
using Sonarr.Http;

namespace NzbDrone.Api.Indexers
{
    public abstract class ReleaseModuleBase : SonarrRestModule<ReleaseResource>
    {
        protected virtual List<ReleaseResource> MapDecisions(IEnumerable<DownloadDecision> decisions)
        {
            var result = new List<ReleaseResource>();

            foreach (var downloadDecision in decisions)
            {
                var release = MapDecision(downloadDecision, result.Count);

                result.Add(release);
            }

            return result;
        }

        protected virtual ReleaseResource MapDecision(DownloadDecision decision, int initialWeight)
        {
            var release = decision.ToResource();

            release.ReleaseWeight = initialWeight;

            if (decision.RemoteEpisode.Series != null)
            {
                release.QualityWeight = decision.RemoteEpisode.Series
                                                              .Profile.Value
                                                              .Items.FindIndex(v => v.Quality == release.Quality.Quality) * 100;
            }

            release.QualityWeight += release.Quality.Revision.Real * 10;
            release.QualityWeight += release.Quality.Revision.Version;

            return release;
        }
    }
}
