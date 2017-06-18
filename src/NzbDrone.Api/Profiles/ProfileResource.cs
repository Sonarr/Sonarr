using System.Collections.Generic;
using System.Linq;
using Sonarr.Http.REST;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.Profiles
{
    public class ProfileResource : RestResource
    {
        public string Name { get; set; }
        public int Cutoff { get; set; }
        public List<ProfileQualityItemResource> Items { get; set; }
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

                // Flatten groups so things don't explode
                Items = model.Items.SelectMany(i =>
                {
                    if (i == null)
                    {
                        return null;
                    }

                    if (i.Items.Any())
                    {
                        return i.Items.ConvertAll(ToResource);
                    }

                    return new List<ProfileQualityItemResource> {ToResource(i)};
                }).ToList()
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
                Cutoff = resource.Cutoff,
                Items = resource.Items.ConvertAll(ToModel)
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
