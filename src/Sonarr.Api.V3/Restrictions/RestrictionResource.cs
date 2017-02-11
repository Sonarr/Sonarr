using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Restrictions;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Restrictions
{
    public class RestrictionResource : RestResource
    {
        public string Required { get; set; }
        public string Preferred { get; set; }
        public string Ignored { get; set; }
        public HashSet<int> Tags { get; set; }

        public RestrictionResource()
        {
            Tags = new HashSet<int>();
        }
    }

    public static class RestrictionResourceMapper
    {
        public static RestrictionResource ToResource(this Restriction model)
        {
            if (model == null) return null;

            return new RestrictionResource
            {
                Id = model.Id,

                Required = model.Required,
                Preferred = model.Preferred,
                Ignored = model.Ignored,
                Tags = new HashSet<int>(model.Tags)
            };
        }

        public static Restriction ToModel(this RestrictionResource resource)
        {
            if (resource == null) return null;

            return new Restriction
            {
                Id = resource.Id,

                Required = resource.Required,
                Preferred = resource.Preferred,
                Ignored = resource.Ignored,
                Tags = new HashSet<int>(resource.Tags)
            };
        }

        public static List<RestrictionResource> ToResource(this IEnumerable<Restriction> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
