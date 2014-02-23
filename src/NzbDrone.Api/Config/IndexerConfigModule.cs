using FluentValidation;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Config
{
    public class IndexerConfigModule : NzbDroneConfigModule<IndexerConfigResource>
    {

        public IndexerConfigModule(IConfigService configService)
            : base(configService)
        {
            SharedValidator.RuleFor(c => c.RssSyncInterval).InclusiveBetween(10, 120);
        }
    }
}