using FluentValidation;
using NzbDrone.Core.Configuration;
using Sonarr.Http;
using Sonarr.Http.Validation;

namespace Sonarr.Api.V3.Config
{
    [V3ApiController("config/indexer")]
    public class IndexerConfigController : ConfigController<IndexerConfigResource>
    {
        public IndexerConfigController(IConfigService configService)
            : base(configService)
        {
            SharedValidator.RuleFor(c => c.MinimumAge)
                           .GreaterThanOrEqualTo(0);

            SharedValidator.RuleFor(c => c.Retention)
                           .GreaterThanOrEqualTo(0);

            SharedValidator.RuleFor(c => c.RssSyncInterval)
                           .IsValidRssSyncInterval();
        }

        protected override IndexerConfigResource ToResource(IConfigService model)
        {
            return IndexerConfigResourceMapper.ToResource(model);
        }
    }
}
