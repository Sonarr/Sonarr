using FluentValidation;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Api.Config
{
    public class DownloadClientConfigModule : NzbDroneConfigModule<DownloadClientConfigResource>
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