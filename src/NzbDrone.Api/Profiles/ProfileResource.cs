using System.Collections.Generic;
using System.Linq;
using Sonarr.Http.REST;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.Profiles
{
    public class ProfileResource : RestResource
    {
        public string Name { get; set; }
        public Quality Cutoff { get; set; }
        public List<ProfileQualityItemResource> Items { get; set; }
        public Language Language { get; set; }
    }

    public class ProfileQualityItemResource : RestResource
    {
        public Quality Quality { get; set; }
        public bool Allowed { get; set; }
    }

    public static class ProfileResourceMapper
    {
        public static ProfileResource ToResource(this Profile model)
        {
            if (model == null) return null;

            return new ProfileResource
            {
                Id = model.Id,

                Name = model.Name,
                Cutoff = model.Cutoff,
                Items = model.Items.ConvertAll(ToResource),
                Language = model.Language
            };
        }

        public static ProfileQualityItemResource ToResource(this ProfileQualityItem model)
        {
            if (model == null) return null;

            return new ProfileQualityItemResource
            {
                Quality = model.Quality,
                Allowed = model.Allowed
            };
        }
            
        public static Profile ToModel(this ProfileResource resource)
        {
            if (resource == null) return null;

            return new Profile
            {
                Id = resource.Id,

                Name = resource.Name,
                Cutoff = (Quality)resource.Cutoff.Id,
                Items = resource.Items.ConvertAll(ToModel),
                Language = resource.Language
            };
        }

        public static ProfileQualityItem ToModel(this ProfileQualityItemResource resource)
        {
            if (resource == null) return null;

            return new ProfileQualityItem
            {
                Quality = (Quality)resource.Quality.Id,
                Allowed = resource.Allowed
            };
        }

        public static List<ProfileResource> ToResource(this IEnumerable<Profile> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}