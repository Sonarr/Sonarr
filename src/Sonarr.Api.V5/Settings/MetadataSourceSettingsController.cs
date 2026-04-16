using FluentValidation;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource;
using Sonarr.Http;

namespace Sonarr.Api.V5.Settings;

[V5ApiController("settings/metadatasource")]
public class MetadataSourceSettingsController : SettingsController<MetadataSourceSettingsResource>
{
    public MetadataSourceSettingsController(IConfigFileProvider configFileProvider, IConfigService configService)
        : base(configFileProvider, configService)
    {
        SharedValidator.RuleFor(c => c.MetadataSource)
                       .IsInEnum();

        SharedValidator.RuleFor(c => c.TmdbApiKey)
                       .NotEmpty()
                       .When(c => c.MetadataSource == MetadataSourceType.Tmdb)
                       .WithMessage("TMDb API key is required when TMDb is selected");
    }

    protected override MetadataSourceSettingsResource ToResource(IConfigFileProvider configFile, IConfigService model)
    {
        return MetadataSourceSettingsResourceMapper.ToResource(model);
    }
}
