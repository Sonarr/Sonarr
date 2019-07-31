using System.Collections.Generic;
using NzbDrone.Core.DecisionEngine;
using Sonarr.Http;

namespace Sonarr.Api.V3.Indexers
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
                release.QualityWeight = decision.RemoteEpisode
                                                .Series
                                                .QualityProfile.Value.GetIndex(release.Quality.Quality).Index * 100;

                release.LanguageWeight = decision.RemoteEpisode
                                                 .Series
                                                 .LanguageProfile.Value
                                                 .Languages.FindIndex(v => v.Language == release.Language) * 100;
            }

            release.QualityWeight += release.Quality.Revision.Real * 10;
            release.QualityWeight += release.Quality.Revision.Version;

            return release;
        }
    }
}
