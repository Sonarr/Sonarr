using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Config
{
    public class UiConfigModule : NzbDroneConfigModule<UiConfigResource>
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