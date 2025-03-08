using System.Linq;
using NLog;
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
    private readonly Logger _logger;

    public RejectedImportService(ICachedIndexerSettingsProvider cachedIndexerSettingsProvider, Logger logger)
    {
        _cachedIndexerSettingsProvider = cachedIndexerSettingsProvider;
        _logger = logger;
    }

    public bool Process(TrackedDownload trackedDownload, ImportResult importResult)
    {
        if (importResult.Result != ImportResultType.Rejected || trackedDownload.RemoteEpisode?.Release == null)
        {
            return false;
        }

        var indexerSettings = _cachedIndexerSettingsProvider.GetSettings(trackedDownload.RemoteEpisode.Release.IndexerId);
        var rejectionReason = importResult.ImportDecision.Rejections.FirstOrDefault()?.Reason;

        if (indexerSettings == null)
        {
            trackedDownload.Warn(new TrackedDownloadStatusMessage(trackedDownload.DownloadItem.Title, importResult.Errors));
            return true;
        }

        if (rejectionReason == ImportRejectionReason.DangerousFile &&
            indexerSettings.FailDownloads.Contains(FailDownloads.PotentiallyDangerous))
        {
            _logger.Trace("Download '{0}' contains potentially dangerous file, marking as failed", trackedDownload.DownloadItem.Title);
            trackedDownload.Fail();
        }
        else if (rejectionReason == ImportRejectionReason.ExecutableFile &&
            indexerSettings.FailDownloads.Contains(FailDownloads.Executables))
        {
            _logger.Trace("Download '{0}' contains executable file, marking as failed", trackedDownload.DownloadItem.Title);
            trackedDownload.Fail();
        }
        else
        {
            trackedDownload.Warn(new TrackedDownloadStatusMessage(trackedDownload.DownloadItem.Title, importResult.Errors));
        }

        return true;
    }
}
