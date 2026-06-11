using FluentValidation;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Languages;
using Sonarr.Http;

namespace Sonarr.Api.V5.Settings;

[V5ApiController("settings/metadata")]
public class MetadataSourceSettingsController : SettingsController<MetadataSourceSettingsResource>
{
    public MetadataSourceSettingsController(IConfigFileProvider configFileProvider, IConfigService configService)
        : base(configFileProvider, configService)
    {
        SharedValidator.RuleFor(c => c.PreferredMetadataLanguage).Custom((value, context) =>
        {
            if (!Language.All.Any(o => o.Id == value))
            {
                context.AddFailure("Invalid Metadata Language value");
            }
        });

        SharedValidator.RuleFor(c => c.PreferredMetadataLanguage)
                       .GreaterThanOrEqualTo(1)
                       .WithMessage("The Metadata Language value cannot be less than 1");
    }

    protected override MetadataSourceSettingsResource ToResource(IConfigFileProvider configFile, IConfigService model)
    {
        return MetadataSourceSettingsResourceMapper.ToResource(model);
    }
}
