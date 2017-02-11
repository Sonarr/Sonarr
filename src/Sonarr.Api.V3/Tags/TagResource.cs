using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Tags;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Tags
{
    public class TagResource : RestResource
    {
        public string Label { get; set; }
    }

    public static class TagResourceMapper
    {
        public static TagResource ToResource(this Tag model)
        {
            if (model == null) return null;

            return new TagResource
            {
                Id = model.Id,
                Label = model.Label
            };
        }

        public static Tag ToModel(this TagResource resource)
        {
            if (resource == null) return null;

            return new Tag
            {
                Id = resource.Id,
                Label = resource.Label
            };
        }

        public static List<TagResource> ToResource(this IEnumerable<Tag> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
