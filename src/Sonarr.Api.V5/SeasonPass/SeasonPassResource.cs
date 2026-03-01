using NzbDrone.Core.Tv;

namespace Sonarr.Api.V5.SeasonPass;

public class SeasonPassResource
{
    public List<SeasonPassSeriesResource> Series { get; set; } = [];
    public MonitoringOptionsResource? MonitoringOptions { get; set; }
}

public class MonitoringOptionsResource
{
    public MonitorTypes Monitor { get; set; }
}

public static class MonitoringOptionsResourceMapper
{
    public static MonitoringOptions ToModel(this MonitoringOptionsResource? resource)
    {
        if (resource == null)
        {
            return new MonitoringOptions();
        }

        return new MonitoringOptions
        {
            Monitor = resource.Monitor
        };
    }
}
