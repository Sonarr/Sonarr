using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using Sonarr.Http;

namespace Sonarr.Api.V3.ImportLists
{
    [V3ApiController]
    public class ImportListController : ProviderControllerBase<ImportListResource, IImportList, ImportListDefinition>
    {
        public static readonly ImportListResourceMapper ResourceMapper = new ImportListResourceMapper();

        public ImportListController(IImportListFactory importListFactory, ProfileExistsValidator profileExistsValidator, LanguageProfileExistsValidator languageProfileExistsValidator)
            : base(importListFactory, "importlist", ResourceMapper)
        {
            Http.Validation.RuleBuilderExtensions.ValidId(SharedValidator.RuleFor(s => s.QualityProfileId));
            Http.Validation.RuleBuilderExtensions.ValidId(SharedValidator.RuleFor(s => s.LanguageProfileId));

            SharedValidator.RuleFor(c => c.RootFolderPath).IsValidPath();
            SharedValidator.RuleFor(c => c.QualityProfileId).SetValidator(profileExistsValidator);
            SharedValidator.RuleFor(c => c.LanguageProfileId).SetValidator(languageProfileExistsValidator);
        }
    }
}
