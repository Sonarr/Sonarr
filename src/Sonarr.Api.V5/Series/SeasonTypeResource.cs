using NzbDrone.Core.Tv;

namespace Sonarr.Api.V5.Series
{
    public class SeasonTypeResource
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public List<int> SeasonNumbers { get; set; } = [];
        public int EpisodeCount { get; set; }
    }

    public static class SeasonTypeResourceMapper
    {
        public static SeasonTypeResource ToResource(this SeasonType model)
        {
            return new SeasonTypeResource
            {
                Name = model.Name,
                Type = model.Type,
                SeasonNumbers = model.SeasonNumbers,
                EpisodeCount = model.EpisodeCount
            };
        }

        public static List<SeasonTypeResource> ToResource(this IEnumerable<SeasonType> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
