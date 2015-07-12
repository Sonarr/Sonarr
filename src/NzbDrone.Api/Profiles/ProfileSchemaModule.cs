using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.Profiles
{
    public class ProfileSchemaModule : NzbDroneRestModule<ProfileResource>
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

            return new List<ProfileResource> { profile.ToResource() };
        }
    }
}