using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Profiles.Releases;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Profiles.Release
{
    public class ReleaseProfileResource : RestResource
    {
        public bool Enabled { get; set; }
        public string Required { get; set; }
        public string Ignored { get; set; }
        public List<KeyValuePair<string, int>> Preferred { get; set; }
        public bool IncludePreferredWhenRenaming { get; set; }
        public int IndexerId { get; set; }
        public HashSet<int> Tags { get; set; }

        public ReleaseProfileResource()
        {
            Tags = new HashSet<int>();
        }
    }

    public static class RestrictionResourceMapper
    {
        public static ReleaseProfileResource ToResource(this ReleaseProfile model)
        {
            if (model == null) return null;

            return new ReleaseProfileResource
            {
                Id = model.Id,

                Enabled = model.Enabled,
                Required = model.Required,
                Ignored = model.Ignored,
                Preferred = model.Preferred,
                IncludePreferredWhenRenaming = model.IncludePreferredWhenRenaming,
                IndexerId = model.IndexerId,
                Tags = new HashSet<int>(model.Tags)
            };
        }

        public static ReleaseProfile ToModel(this ReleaseProfileResource resource)
        {
            if (resource == null) return null;

            return new ReleaseProfile
            {
                Id = resource.Id,

                Enabled = resource.Enabled,
                Required = resource.Required,
                Ignored = resource.Ignored,
                Preferred = resource.Preferred,
                IncludePreferredWhenRenaming = resource.IncludePreferredWhenRenaming,
                IndexerId = resource.IndexerId,
                Tags = new HashSet<int>(resource.Tags)
            };
        }

        public static List<ReleaseProfileResource> ToResource(this IEnumerable<ReleaseProfile> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
