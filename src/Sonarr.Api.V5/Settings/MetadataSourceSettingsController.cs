using FluentValidation;
using NzbDrone.Core.Configuration;
using Sonarr.Http;

namespace Sonarr.Api.V5.Settings
{
    [V5ApiController("settings/metadatasource")]
    public class MetadataSourceSettingsController : SettingsController<MetadataSourceSettingsResource>
    {
        public MetadataSourceSettingsController(IConfigFileProvider configFileProvider, IConfigService configService)
            : base(configFileProvider, configService)
        {
            SharedValidator.RuleFor(c => c.TmdbApiKey).MaximumLength(500)
                           .WithMessage("The TMDB API-Key cannot be longer than 500 characters");
        }

        protected override MetadataSourceSettingsResource ToResource(IConfigFileProvider configFile, IConfigService model)
        {
            return MetadataSourceSettingsResourceMapper.ToResource(configFile, model);
        }
    }
}
