using System;
using System.Text.RegularExpressions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.HealthCheck
{
    public class HealthCheck : ModelBase
    {
        private static readonly Regex CleanFragmentRegex = new Regex("[^a-z ]", RegexOptions.Compiled);

        public Type Source { get; set; }
        public HealthCheckResult Type { get; set; }
        public HealthCheckReason Reason { get; set; }
        public string Message { get; set; }
        public HttpUri WikiUrl { get; set; }

        public HealthCheck()
        {
        }

        public HealthCheck(Type source)
        {
            Source = source;
            Type = HealthCheckResult.Ok;
        }

        public HealthCheck(Type source, HealthCheckResult type, HealthCheckReason reason, string message, string wikiFragment = null)
        {
            Source = source;
            Type = type;
            Reason = reason;
            Message = message;
            WikiUrl = MakeWikiUrl(wikiFragment ?? MakeWikiFragment(message));
        }

        private static string MakeWikiFragment(string message)
        {
            return "#" + CleanFragmentRegex.Replace(message.ToLower(), string.Empty).Replace(' ', '-');
        }

        private static HttpUri MakeWikiUrl(string fragment)
        {
            return new HttpUri("https://wiki.servarr.com/sonarr/system") + new HttpUri(fragment);
        }
    }

    public enum HealthCheckResult
    {
        Ok = 0,
        Notice = 1,
        Warning = 2,
        Error = 3
    }

    public enum HealthCheckReason
    {
        AppDataLocation,
        DownloadClientCheckNoneAvailable,
        DownloadClientCheckUnableToCommunicate,
        DownloadClientRemovesCompletedDownloads,
        DownloadClientRootFolder,
        DownloadClientSorting,
        DownloadClientStatusAllClients,
        DownloadClientStatusSingleClient,
        ImportListRootFolderMissing,
        ImportListRootFolderMultipleMissing,
        ImportListStatusAllUnavailable,
        ImportListStatusUnavailable,
        ImportMechanismEnableCompletedDownloadHandlingIfPossible,
        ImportMechanismEnableCompletedDownloadHandlingIfPossibleMultiComputer,
        ImportMechanismHandlingDisabled,
        IndexerDownloadClient,
        IndexerJackettAll,
        IndexerLongTermStatusAllUnavailable,
        IndexerLongTermStatusUnavailable,
        IndexerRssNoIndexersAvailable,
        IndexerRssNoIndexersEnabled,
        IndexerSearchNoAutomatic,
        IndexerSearchNoAvailableIndexers,
        IndexerSearchNoInteractive,
        IndexerStatusAllUnavailable,
        IndexerStatusUnavailable,
        MinimumApiKeyLength,
        MountSeries,
        NotificationStatusAll,
        NotificationStatusSingle,
        Package,
        ProxyBadRequest,
        ProxyFailed,
        ProxyResolveIp,
        RecycleBinUnableToWrite,
        RemotePathMappingBadDockerPath,
        RemotePathMappingDockerFolderMissing,
        RemotePathMappingDownloadPermissionsEpisode,
        RemotePathMappingFileRemoved,
        RemotePathMappingFilesBadDockerPath,
        RemotePathMappingFilesGenericPermissions,
        RemotePathMappingFilesLocalWrongOSPath,
        RemotePathMappingFilesWrongOSPath,
        RemotePathMappingFolderPermissions,
        RemotePathMappingGenericPermissions,
        RemotePathMappingImportEpisodeFailed,
        RemotePathMappingLocalFolderMissing,
        RemotePathMappingLocalWrongOSPath,
        RemotePathMappingRemoteDownloadClient,
        RemotePathMappingWrongOSPath,
        RemovedSeriesMultiple,
        RemovedSeriesSingle,
        RootFolderMissing,
        RootFolderMultipleMissing,
        ServerNotification,
        SystemTime,
        UpdateAvailable,
        UpdateStartupNotWritable,
        UpdateStartupTranslocation,
        UpdateUiNotWritable
    }
}
