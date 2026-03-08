using FluentValidation;
using NzbDrone.Core.Configuration;
using Sonarr.Http;
using Sonarr.Http.Validation;

namespace Sonarr.Api.V5.Settings
{
    [V5ApiController("settings/indexer")]
    public class IndexerSettingsController : SettingsController<IndexerSettingsResource>
    {
        public IndexerSettingsController(IConfigFileProvider configFileProvider,
            IConfigService configService)
            : base(configFileProvider, configService)
        {
            SharedValidator.RuleFor(c => c.MinimumAge)
                           .GreaterThanOrEqualTo(0);

            SharedValidator.RuleFor(c => c.Retention)
                           .GreaterThanOrEqualTo(0);

            SharedValidator.RuleFor(c => c.RssSyncInterval)
                           .IsValidRssSyncInterval();
        }

        protected override IndexerSettingsResource ToResource(IConfigFileProvider configFile, IConfigService model)
        {
            return IndexerConfigResourceMapper.ToResource(model);
        }
    }
}
