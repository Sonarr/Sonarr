using NzbDrone.Core.Configuration;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.Settings
{
    public class IndexerSettingsResource : RestResource
    {
        public int MinimumAge { get; set; }
        public int Retention { get; set; }
        public int MaximumSize { get; set; }
        public int RssSyncInterval { get; set; }
    }

    public static class IndexerConfigResourceMapper
    {
        public static IndexerSettingsResource ToResource(IConfigService model)
        {
            return new IndexerSettingsResource
            {
                MinimumAge = model.MinimumAge,
                Retention = model.Retention,
                MaximumSize = model.MaximumSize,
                RssSyncInterval = model.RssSyncInterval
            };
        }
    }
}
