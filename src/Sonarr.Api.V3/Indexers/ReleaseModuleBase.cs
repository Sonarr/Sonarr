using System.Collections.Generic;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Profiles.Languages;
using NzbDrone.Core.Profiles.Qualities;
using Sonarr.Http;

namespace Sonarr.Api.V3.Indexers
{
    public abstract class ReleaseModuleBase : SonarrRestModule<ReleaseResource>
    {
        private readonly LanguageProfile LANGUAGE_PROFILE;
        private readonly QualityProfile QUALITY_PROFILE;

        public ReleaseModuleBase(ILanguageProfileService languageProfileService,
                                 IQualityProfileService qualityProfileService)
        {
            LANGUAGE_PROFILE = languageProfileService.GetDefaultProfile(string.Empty);
            QUALITY_PROFILE = qualityProfileService.GetDefaultProfile(string.Empty);
        }

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

            release.QualityWeight = QUALITY_PROFILE.GetIndex(release.Quality.Quality).Index * 100;
            release.LanguageWeight = LANGUAGE_PROFILE.Languages.FindIndex(v => v.Language == release.Language) * 100;

            release.QualityWeight += release.Quality.Revision.Real * 10;
            release.QualityWeight += release.Quality.Revision.Version;

            return release;
        }
    }
}
