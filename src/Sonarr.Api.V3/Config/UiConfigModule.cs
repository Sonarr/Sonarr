using System.Linq;
using System.Reflection;
using NzbDrone.Core.Configuration;
using Sonarr.Http;

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