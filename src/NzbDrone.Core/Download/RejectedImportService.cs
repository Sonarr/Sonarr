using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.MediaFiles.EpisodeImport;

namespace NzbDrone.Core.Download;

public interface IRejectedImportService
{
    bool Process(TrackedDownload trackedDownload, ImportResult importResult);
}

public class RejectedImportService : IRejectedImportService
{
    private readonly ICachedIndexerSettingsProvider _cachedIndexerSettingsProvider;

    public RejectedImportService(ICachedIndexerSettingsProvider cachedIndexerSettingsProvider)
    {
        _cachedIndexerSettingsProvider = cachedIndexerSettingsProvider;
    }

    public bool Process(TrackedDownload trackedDownload, ImportResult importResult)
    {
        if (importResult.Result != ImportResultType.Rejected || importResult.ImportDecision.LocalEpisode != null)
        {
            return false;
        }

        var indexerSettings = _cachedIndexerSettingsProvider.GetSettings(trackedDownload.RemoteEpisode.Release.IndexerId);
        var rejectionReason = importResult.ImportDecision.Rejections.FirstOrDefault()?.Reason;

        if (rejectionReason == ImportRejectionReason.DangerousFile &&
            indexerSettings.FailDownloads.Contains(FailDownloads.PotentiallyDangerous))
        {
            trackedDownload.Fail();
        }
        else if (rejectionReason == ImportRejectionReason.ExecutableFile &&
            indexerSettings.FailDownloads.Contains(FailDownloads.Executables))
        {
            trackedDownload.Fail();
        }
        else
        {
            trackedDownload.Warn(new TrackedDownloadStatusMessage(importResult.Errors.First(), new List<string>()));
        }

        return true;
    }
}
