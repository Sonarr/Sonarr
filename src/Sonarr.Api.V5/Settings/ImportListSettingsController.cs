using FluentValidation;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Validation;
using Sonarr.Http;

namespace Sonarr.Api.V5.Settings;

[V5ApiController("settings/importlist")]
public class ImportListSettingsController : SettingsController<ImportListSettingsResource>
{
    public ImportListSettingsController(IConfigFileProvider configFileProvider,
        IConfigService configService)
        : base(configFileProvider, configService)
    {
        SharedValidator.RuleFor(c => c.ListSyncTag)
                       .ValidId()
                       .WithMessage("Tag must be specified")
                       .When(c => c.ListSyncLevel == ListSyncLevelType.KeepAndTag);
    }

    protected override ImportListSettingsResource ToResource(IConfigFileProvider configFile, IConfigService model)
    {
        return ImportListSettingsResourceMapper.ToResource(model);
    }
}
