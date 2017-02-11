using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using Sonarr.Http;
using Sonarr.Http.Mapping;

namespace NzbDrone.Api.Profiles
{
    public class ProfileSchemaModule : SonarrRestModule<ProfileResource>
    {
        private readonly IQualityDefinitionService _qualityDefinitionService;

        public ProfileSchemaModule(IQualityDefinitionService qualityDefinitionService)
            : base("/profile/schema")
        {
            _qualityDefinitionService = qualityDefinitionService;

            GetResourceAll = GetAll;
        }

        private List<ProfileResource> GetAll()
        {
            var items = _qualityDefinitionService.All()
                .OrderBy(v => v.Weight)
                .Select(v => new ProfileQualityItem { Quality = v.Quality, Allowed = false })
                .ToList();

            var profile = new Profile();
            profile.Cutoff = Quality.Unknown;
            profile.Items = items;
            profile.Language = Language.English;

            return new List<ProfileResource> { profile.ToResource() };
        }
    }
}