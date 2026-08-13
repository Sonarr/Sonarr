using NzbDrone.Core.Configuration;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.Settings
{
    public class MetadataSourceSettingsResource : RestResource
    {
        public string? TmdbApiKey { get; set; }
        public bool TmdbEnabled { get; set; }
    }

    public static class MetadataSourceSettingsResourceMapper
    {
        public static MetadataSourceSettingsResource ToResource(IConfigFileProvider config, IConfigService model)
        {
            return new MetadataSourceSettingsResource
            {
                TmdbApiKey = model.TmdbApiKey,
                TmdbEnabled = model.TmdbEnabled
            };
        }
    }
}
