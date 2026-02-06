using NzbDrone.Core.Configuration;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.Config;

/// <summary>
/// Resource for metadata source (TVDB via SkyHook) settings.
/// </summary>
public class MetadataSourceSettingsResource : RestResource
{
    public string TvdbMetadataLanguage { get; set; } = "en";
}

public static class MetadataSourceSettingsResourceMapper
{
    public static MetadataSourceSettingsResource ToResource(IConfigService model)
    {
        return new MetadataSourceSettingsResource
        {
            TvdbMetadataLanguage = model.TvdbMetadataLanguage ?? "en"
        };
    }
}
