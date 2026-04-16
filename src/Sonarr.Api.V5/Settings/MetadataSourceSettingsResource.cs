using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.Settings;

public class MetadataSourceSettingsResource : RestResource
{
    public MetadataSourceType MetadataSource { get; set; }
    public string? TmdbApiKey { get; set; }
}

public static class MetadataSourceSettingsResourceMapper
{
    public static MetadataSourceSettingsResource ToResource(IConfigService configService)
    {
        return new MetadataSourceSettingsResource
        {
            MetadataSource = configService.MetadataSource,
            TmdbApiKey = configService.TmdbApiKey
        };
    }
}
