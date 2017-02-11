using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Tv;

namespace Sonarr.Api.V3.Series
{
    public class SeasonResource
    {
        public int SeasonNumber { get; set; }
        public bool Monitored { get; set; }
        public SeasonStatisticsResource Statistics { get; set; }
    }

    public static class SeasonResourceMapper
    {
        public static SeasonResource ToResource(this Season model)
        {
            if (model == null) return null;

            return new SeasonResource
            {
                SeasonNumber = model.SeasonNumber,
                Monitored = model.Monitored
            };
        }

        public static Season ToModel(this SeasonResource resource)
        {
            if (resource == null) return null;

            return new Season
            {
                SeasonNumber = resource.SeasonNumber,
                Monitored = resource.Monitored
            };
        }

        public static List<SeasonResource> ToResource(this IEnumerable<Season> models)
        {
            return models.Select(ToResource).ToList();
        }

        public static List<Season> ToModel(this IEnumerable<SeasonResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
