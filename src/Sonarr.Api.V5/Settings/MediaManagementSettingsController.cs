using FluentValidation;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using Sonarr.Http;

namespace Sonarr.Api.V5.Settings;

[V5ApiController("settings/mediamanagement")]
public class MediaManagementSettingsController : SettingsController<MediaManagementSettingsResource>
{
    public MediaManagementSettingsController(IConfigService configService,
                                       PathExistsValidator pathExistsValidator,
                                       FolderChmodValidator folderChmodValidator,
                                       FolderWritableValidator folderWritableValidator,
                                       SeriesPathValidator seriesPathValidator,
                                       StartupFolderValidator startupFolderValidator,
                                       SystemFolderValidator systemFolderValidator,
                                       RootFolderAncestorValidator rootFolderAncestorValidator,
                                       RootFolderValidator rootFolderValidator)
        : base(configService)
    {
        SharedValidator.RuleFor(c => c.RecycleBinCleanupDays).GreaterThanOrEqualTo(0);
        SharedValidator.RuleFor(c => c.ChmodFolder).SetValidator(folderChmodValidator).When(c => !string.IsNullOrEmpty(c.ChmodFolder) && (OsInfo.IsLinux || OsInfo.IsOsx));

        SharedValidator.RuleFor(c => c.RecycleBin).IsValidPath()
                                                  .SetValidator(folderWritableValidator)
                                                  .SetValidator(rootFolderValidator)
                                                  .SetValidator(pathExistsValidator)
                                                  .SetValidator(rootFolderAncestorValidator)
                                                  .SetValidator(startupFolderValidator)
                                                  .SetValidator(systemFolderValidator)
                                                  .SetValidator(seriesPathValidator)
                                                  .When(c => !string.IsNullOrWhiteSpace(c.RecycleBin));

        SharedValidator.RuleFor(c => c.ScriptImportPath).IsValidPath().When(c => c.UseScriptImport);

        SharedValidator.RuleFor(c => c.MinimumFreeSpaceWhenImporting).GreaterThanOrEqualTo(100);

        SharedValidator.RuleFor(c => c.UserRejectedExtensions).Custom((extensions, context) =>
        {
            var userRejectedExtensions = extensions?.Split([','], StringSplitOptions.RemoveEmptyEntries)
                                                            .Select(e => e.Trim(' ', '.')
                                                            .Insert(0, "."))
                                                            .ToList() ?? [];

            var matchingArchiveExtensions = userRejectedExtensions.Where(ext => FileExtensions.ArchiveExtensions.Contains(ext)).ToList();

            if (matchingArchiveExtensions.Count > 0)
            {
                context.AddFailure($"Rejected extensions may not include valid archive extensions: {string.Join(", ", matchingArchiveExtensions)}");
            }

            var matchingMediaFileExtensions = userRejectedExtensions.Where(ext => MediaFileExtensions.Extensions.Contains(ext)).ToList();

            if (matchingMediaFileExtensions.Count > 0)
            {
                context.AddFailure($"Rejected extensions may not include valid media file extensions: {string.Join(", ", matchingMediaFileExtensions)}");
            }
        });
    }

    protected override MediaManagementSettingsResource ToResource(IConfigService model)
    {
        return MediaManagementConfigResourceMapper.ToResource(model);
    }
}
