using System.Collections.Generic;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Profiles.Languages;
using NzbDrone.Core.Profiles.Qualities;
using Sonarr.Http;

namespace Sonarr.Api.V3.Indexers
{
    public abstract class ReleaseModuleBase : SonarrRestModule<ReleaseResource>
    {
        private readonly LanguageProfile _languageProfile;
        private readonly QualityProfile _qualityProfile;

        public ReleaseModuleBase(ILanguageProfileService languageProfileService,
                                 IQualityProfileService qualityProfileService)
        {
            _languageProfile = languageProfileService.GetDefaultProfile(string.Empty);
            _qualityProfile = qualityProfileService.GetDefaultProfile(string.Empty);
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

            release.QualityWeight = _qualityProfile.GetIndex(release.Quality.Quality).Index * 100;
            release.LanguageWeight = _languageProfile.Languages.FindIndex(v => v.Language == release.Language) * 100;

            release.QualityWeight += release.Quality.Revision.Real * 10;
            release.QualityWeight += release.Quality.Revision.Version;

            return release;
        }
    }
}
