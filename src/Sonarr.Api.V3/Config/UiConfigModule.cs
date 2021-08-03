using NzbDrone.Core.Configuration;

namespace Sonarr.Api.V3.Config
{
    public class UiConfigModule : SonarrConfigModule<UiConfigResource>
    {
        public UiConfigModule(IConfigService configService)
            : base(configService)
        {
        }

        protected override UiConfigResource ToResource(IConfigService model)
        {
            return UiConfigResourceMapper.ToResource(model);
        }
    }
}
