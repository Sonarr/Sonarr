using NzbDrone.Core.Configuration;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.Settings;

public class MetadataSourceSettingsResource : RestResource
{
    public int PreferredMetadataLanguage { get; set; }
}

public static class MetadataSourceSettingsResourceMapper
{
    public static MetadataSourceSettingsResource ToResource(IConfigService model)
    {
        return new MetadataSourceSettingsResource
        {
            PreferredMetadataLanguage = model.PreferredMetadataLanguage
        };
    }
}
