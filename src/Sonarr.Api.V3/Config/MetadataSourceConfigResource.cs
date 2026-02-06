using NzbDrone.Core.Configuration;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Config
{
    public class MetadataSourceConfigResource : RestResource
    {
        public string TvdbMetadataLanguage { get; set; } = "en";
    }

    public static class MetadataSourceConfigResourceMapper
    {
        public static MetadataSourceConfigResource ToResource(IConfigService model)
        {
            return new MetadataSourceConfigResource
            {
                TvdbMetadataLanguage = model.TvdbMetadataLanguage ?? "en"
            };
        }
    }
}
