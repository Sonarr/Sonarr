using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Tv;

namespace Sonarr.Api.V3.Series
{
    public class SeasonResource
    {
        public int SeasonNumber { get; set; }
        public bool Monitored { get; set; }
        public SeasonStatisticsResource Statistics { get; set; }
        public List<MediaCover> Images { get; set; }
    }

    public static class SeasonResourceMapper
    {
        public static SeasonResource ToResource(this Season model, bool includeImages = false)
        {
            if (model == null) return null;

            return new SeasonResource
            {
                SeasonNumber = model.SeasonNumber,
                Monitored = model.Monitored,
                Images = includeImages ? model.Images : null
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

        public static List<SeasonResource> ToResource(this IEnumerable<Season> models, bool includeImages = false)
        {
            return models.Select(s => ToResource(s, includeImages)).ToList();
        }

        public static List<Season> ToModel(this IEnumerable<SeasonResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
