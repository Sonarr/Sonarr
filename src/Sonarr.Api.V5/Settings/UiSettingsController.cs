using FluentValidation;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Languages;
using Sonarr.Http;

namespace Sonarr.Api.V5.Settings;

[V5ApiController("settings/ui")]
public class UiSettingsController : SettingsController<UiSettingsResource>
{
    public UiSettingsController(IConfigFileProvider configFileProvider, IConfigService configService)
        : base(configFileProvider, configService)
    {
        SharedValidator.RuleFor(c => c.UiLanguage).Custom((value, context) =>
        {
            if (!Language.All.Any(o => o.Id == value))
            {
                context.AddFailure("Invalid UI Language value");
            }
        });

        SharedValidator.RuleFor(c => c.UiLanguage)
                       .GreaterThanOrEqualTo(1)
                       .WithMessage("The UI Language value cannot be less than 1");
    }

    protected override UiSettingsResource ToResource(IConfigFileProvider configFile, IConfigService model)
    {
        return UiSettingsResourceMapper.ToResource(configFile, model);
    }
}
