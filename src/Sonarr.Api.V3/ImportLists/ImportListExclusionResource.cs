using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.ImportLists.Exclusions;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.ImportLists
{
    public class ImportListExclusionResource : RestResource
    {
        public int TvdbId { get; set; }
        public string Title { get; set; }
    }

    public static class ImportListExclusionResourceMapper
    {
        public static ImportListExclusionResource ToResource(this ImportListExclusion model)
        {
            if (model == null)
            {
                return null;
            }

            return new ImportListExclusionResource
            {
                Id = model.Id,
                TvdbId = model.TvdbId,
                Title = model.Title,
            };
        }

        public static ImportListExclusion ToModel(this ImportListExclusionResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new ImportListExclusion
            {
                Id = resource.Id,
                TvdbId = resource.TvdbId,
                Title = resource.Title
            };
        }

        public static List<ImportListExclusionResource> ToResource(this IEnumerable<ImportListExclusion> filters)
        {
            return filters.Select(ToResource).ToList();
        }
    }
}
