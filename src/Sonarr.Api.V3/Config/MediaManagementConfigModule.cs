using FluentValidation;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Validation.Paths;

namespace Sonarr.Api.V3.Config
{
    public class MediaManagementConfigModule : SonarrConfigModule<MediaManagementConfigResource>
    {
        public MediaManagementConfigModule(IConfigService configService, PathExistsValidator pathExistsValidator)
            : base(configService)
        {
            SharedValidator.RuleFor(c => c.FileChmod).NotEmpty();
            SharedValidator.RuleFor(c => c.FolderChmod).NotEmpty();
            SharedValidator.RuleFor(c => c.RecycleBin).IsValidPath().SetValidator(pathExistsValidator).When(c => !string.IsNullOrWhiteSpace(c.RecycleBin));
        }

        protected override MediaManagementConfigResource ToResource(IConfigService model)
        {
            return MediaManagementConfigResourceMapper.ToResource(model);
        }
    }
}