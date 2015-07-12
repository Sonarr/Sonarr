using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Profiles.Languages;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Profiles.Language
{
    public class LanguageProfileResource : RestResource
    {
        public string Name { get; set; }
        public NzbDrone.Core.Languages.Language Cutoff { get; set; }
        public List<ProfileLanguageItemResource> Languages { get; set; }
    }

    public class ProfileLanguageItemResource : RestResource
    {
        public NzbDrone.Core.Languages.Language Language { get; set; }
        public bool Allowed { get; set; }
    }

    public static class LanguageProfileResourceMapper
    {
        public static LanguageProfileResource ToResource(this LanguageProfile model)
        {
            if (model == null) return null;

            return new LanguageProfileResource
            {
                Id = model.Id,
                Name = model.Name,
                Cutoff = model.Cutoff,
                Languages = model.Languages.ConvertAll(ToResource)
            };
        }

        public static ProfileLanguageItemResource ToResource(this ProfileLanguageItem model)
        {
            if (model == null) return null;

            return new ProfileLanguageItemResource
            {
                Language = model.Language,
                Allowed = model.Allowed
            };
        }

        public static LanguageProfile ToModel(this LanguageProfileResource resource)
        {
            if (resource == null) return null;

            return new LanguageProfile
            {
                Id = resource.Id,
                Name = resource.Name,
                Cutoff = (NzbDrone.Core.Languages.Language)resource.Cutoff.Id,
                Languages = resource.Languages.ConvertAll(ToModel)
            };
        }

        public static ProfileLanguageItem ToModel(this ProfileLanguageItemResource resource)
        {
            if (resource == null) return null;

            return new ProfileLanguageItem
            {
                Language = (NzbDrone.Core.Languages.Language)resource.Language.Id,
                Allowed = resource.Allowed
            };
        }

        public static List<LanguageProfileResource> ToResource(this IEnumerable<LanguageProfile> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
