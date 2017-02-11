using Sonarr.Http.REST;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Config
{
    public class IndexerConfigResource : RestResource
    {
        public int MinimumAge { get; set; }
        public int Retention { get; set; }
        public int RssSyncInterval { get; set; }
    }

    public static class IndexerConfigResourceMapper
    {
        public static IndexerConfigResource ToResource(IConfigService model)
        {
            return new IndexerConfigResource
            {
                MinimumAge = model.MinimumAge,
                Retention = model.Retention,
                RssSyncInterval = model.RssSyncInterval,
            };
        }
    }
}
