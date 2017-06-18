using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using Sonarr.Http.REST;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Api.Profiles
{
    public class ProfileResource : RestResource
    {
        public string Name { get; set; }
        public Quality Cutoff { get; set; }
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

            var cutoffItem = model.Items.First(q =>
            {
                if (q.Id == model.Cutoff) return true;

                if (q.Quality == null) return false;

                return q.Quality.Id == model.Cutoff;
            });

            var cutoff = cutoffItem.Items == null || cutoffItem.Items.Empty()
                ? cutoffItem.Quality
                : cutoffItem.Items.First().Quality;

            return new ProfileResource
            {
                Id = model.Id,

                Name = model.Name,
                Cutoff = cutoff,

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
                Cutoff = resource.Cutoff.Id,
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
