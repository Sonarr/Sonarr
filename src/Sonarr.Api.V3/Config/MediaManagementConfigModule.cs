using FluentValidation;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;

namespace Sonarr.Api.V3.Config
{
    public class MediaManagementConfigModule : SonarrConfigModule<MediaManagementConfigResource>
    {
        public MediaManagementConfigModule(IConfigService configService, PathExistsValidator pathExistsValidator, FolderChmodValidator folderChmodValidator)
            : base(configService)
        {
            SharedValidator.RuleFor(c => c.RecycleBinCleanupDays).GreaterThanOrEqualTo(0);
            SharedValidator.RuleFor(c => c.ChmodFolder).SetValidator(folderChmodValidator).When(c => !string.IsNullOrEmpty(c.ChmodFolder) && OsInfo.IsNotWindows);
            SharedValidator.RuleFor(c => c.RecycleBin).IsValidPath().SetValidator(pathExistsValidator).When(c => !string.IsNullOrWhiteSpace(c.RecycleBin));
            SharedValidator.RuleFor(c => c.MinimumFreeSpaceWhenImporting).GreaterThanOrEqualTo(100);
        }

        protected override MediaManagementConfigResource ToResource(IConfigService model)
        {
            return MediaManagementConfigResourceMapper.ToResource(model);
        }
    }
}
