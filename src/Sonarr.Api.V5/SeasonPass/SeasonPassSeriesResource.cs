using Sonarr.Api.V5.Series;

namespace Sonarr.Api.V5.SeasonPass;

public class SeasonPassSeriesResource
{
    public int Id { get; set; }
    public bool? Monitored { get; set; }
    public List<SeasonResource> Seasons { get; set; } = [];
}
