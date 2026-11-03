using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;
using Sonarr.Http;
using Sonarr.Http.Authentication;

namespace Sonarr.Api.V3.Config
{
    [V3ApiController("config/downloadclient")]
    [RequiresPermission(SonarrRole.Admin)]
    public class DownloadClientConfigController : ConfigController<DownloadClientConfigResource>
    {
        public DownloadClientConfigController(IConfigService configService)
            : base(configService)
        {
        }

        protected override DownloadClientConfigResource ToResource(IConfigService model)
        {
            return DownloadClientConfigResourceMapper.ToResource(model);
        }
    }
}
