using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Profiles.Releases;
using Sonarr.Http.REST;

namespace NzbDrone.Api.Restrictions
{
    public class RestrictionResource : RestResource
    {
        public string Required { get; set; }
        public string Ignored { get; set; }
        public HashSet<int> Tags { get; set; }

        public RestrictionResource()
        {
            Tags = new HashSet<int>();
        }
    }

    public static class RestrictionResourceMapper
    {
        public static RestrictionResource ToResource(this ReleaseProfile model)
        {
            if (model == null) return null;

            return new RestrictionResource
            {
                Id = model.Id,

                Required = model.Required,
                Ignored = model.Ignored,
                Tags = new HashSet<int>(model.Tags)
            };
        }

        public static ReleaseProfile ToModel(this RestrictionResource resource)
        {
            if (resource == null) return null;

            return new ReleaseProfile
            {
                Id = resource.Id,

                Required = resource.Required,
                Ignored = resource.Ignored,
                Tags = new HashSet<int>(resource.Tags)
            };
        }

        public static List<RestrictionResource> ToResource(this IEnumerable<ReleaseProfile> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
