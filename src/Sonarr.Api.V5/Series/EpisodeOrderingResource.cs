using NzbDrone.Core.Tv;

namespace Sonarr.Api.V5.Series;

public class EpisodeOrderingResource
{
    public EpisodeOrderType Type { get; set; }
    public string? Name { get; set; }
}
