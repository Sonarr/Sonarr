using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Tv;

namespace Sonarr.Api.V3.Series
{
    public class SeasonTypeResource
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public List<int> SeasonNumbers { get; set; }
        public int EpisodeCount { get; set; }
    }

    public static class SeasonTypeResourceMapper
    {
        public static SeasonTypeResource ToResource(this SeasonType model)
        {
            if (model == null)
            {
                return null;
            }

            return new SeasonTypeResource
            {
                Name = model.Name,
                Type = model.Type,
                SeasonNumbers = model.SeasonNumbers,
                EpisodeCount = model.EpisodeCount
            };
        }

        public static SeasonType ToModel(this SeasonTypeResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new SeasonType
            {
                Name = resource.Name,
                Type = resource.Type,
                SeasonNumbers = resource.SeasonNumbers,
                EpisodeCount = resource.EpisodeCount
            };
        }

        public static List<SeasonTypeResource> ToResource(this IEnumerable<SeasonType> models)
        {
            return models.Select(ToResource).ToList();
        }

        public static List<SeasonType> ToModel(this IEnumerable<SeasonTypeResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
