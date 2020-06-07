using FluentValidation;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Api.Config
{
    public class MediaManagementConfigModule : NzbDroneConfigModule<MediaManagementConfigResource>
    {
        public MediaManagementConfigModule(IConfigService configService, PathExistsValidator pathExistsValidator, FileChmodValidator fileChmodValidator)
            : base(configService)
        {
            SharedValidator.RuleFor(c => c.FileChmod).SetValidator(fileChmodValidator).When(c => !string.IsNullOrEmpty(c.FileChmod));
            SharedValidator.RuleFor(c => c.RecycleBin).IsValidPath().SetValidator(pathExistsValidator).When(c => !string.IsNullOrWhiteSpace(c.RecycleBin));
        }

        protected override MediaManagementConfigResource ToResource(IConfigService model)
        {
            return MediaManagementConfigResourceMapper.ToResource(model);
        }
    }
}