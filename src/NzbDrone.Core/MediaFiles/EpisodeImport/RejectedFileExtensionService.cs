using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.MediaFiles.EpisodeImport;

public interface IRejectedFileExtensionService
{
    ImportRejection GetRejection(IEnumerable<string> files);
}

public class RejectedFileExtensionService : IRejectedFileExtensionService
{
    private readonly IConfigService _configService;

    public RejectedFileExtensionService(IConfigService configService)
    {
        _configService = configService;
    }

    public ImportRejection GetRejection(IEnumerable<string> files)
    {
        var extensions = files.Select(Path.GetExtension)
                              .Where(e => e.IsNotNullOrWhiteSpace())
                              .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var dangerousExtensions = extensions.Where(FileExtensions.DangerousExtensions.Contains).ToList();

        if (dangerousExtensions.Any())
        {
            return new ImportRejection(ImportRejectionReason.DangerousFile, $"Caution: Found potentially dangerous file with extension: {string.Join(", ", dangerousExtensions)}");
        }

        var executableExtensions = extensions.Where(FileExtensions.ExecutableExtensions.Contains).ToList();

        if (executableExtensions.Any())
        {
            return new ImportRejection(ImportRejectionReason.ExecutableFile, $"Caution: Found executable file with extension: {string.Join(", ", executableExtensions)}");
        }

        var userRejectedExtensions = FileExtensions.ParseUserRejectedExtensions(_configService.UserRejectedExtensions);
        var rejectedExtensions = extensions.Where(userRejectedExtensions.Contains).ToList();

        if (rejectedExtensions.Any())
        {
            return new ImportRejection(ImportRejectionReason.UserRejectedExtension, $"Caution: Found file with user defined rejected extension: {string.Join(", ", rejectedExtensions)}");
        }

        return null;
    }
}
