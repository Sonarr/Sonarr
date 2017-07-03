using NzbDrone.Core.Configuration;

namespace Sonarr.Api.V3.Config
{
    public class DownloadClientConfigModule : SonarrConfigModule<DownloadClientConfigResource>
    {
        public DownloadClientConfigModule(IConfigService configService)
            : base(configService)
        {
        }

        protected override DownloadClientConfigResource ToResource(IConfigService model)
        {
            return DownloadClientConfigResourceMapper.ToResource(model);
        }
    }
}