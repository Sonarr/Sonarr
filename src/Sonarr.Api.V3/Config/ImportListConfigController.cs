using FluentValidation;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Validation;
using Sonarr.Http;

namespace Sonarr.Api.V3.Config
{
    [V3ApiController("config/importlist")]

    public class ImportListConfigController : ConfigController<ImportListConfigResource>
    {
        public ImportListConfigController(IConfigService configService)
            : base(configService)
        {
            SharedValidator.RuleFor(x => x.ListSyncTag)
               .ValidId()
               .WithMessage("Tag must be specified")
               .When(x => x.ListSyncLevel == ListSyncLevelType.KeepAndTag);
        }

        protected override ImportListConfigResource ToResource(IConfigService model)
        {
            return ImportListConfigResourceMapper.ToResource(model);
        }
    }
}
