using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.Intrinsics.X86;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Localization;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ProviderUpdatedEvent<IDownloadClient>))]
    [CheckOn(typeof(ProviderDeletedEvent<IDownloadClient>))]
    [CheckOn(typeof(ModelEvent<RemotePathMapping>))]
    [CheckOn(typeof(EpisodeImportFailedEvent), CheckOnCondition.SuccessfulOnly)]
    public class RemotePathMappingCheck : HealthCheckBase, IProvideHealthCheck
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly IConfigService _configService;
        private readonly Logger _logger;
        private readonly IOsInfo _osInfo;

        public RemotePathMappingCheck(IDiskProvider diskProvider,
                                      IProvideDownloadClient downloadClientProvider,
                                      IConfigService configService,
                                      IOsInfo osInfo,
                                      Logger logger)
        {
            _diskProvider = diskProvider;
            _downloadClientProvider = downloadClientProvider;
            _configService = configService;
            _logger = logger;
            _osInfo = osInfo;
        }

        public override HealthCheck Check()
        {
            // We don't care about client folders if we are not handling completed files
            if (!_configService.EnableCompletedDownloadHandling)
            {
                return new HealthCheck(GetType());
            }

            // Only check clients not in failure status, those get another message
            var clients = _downloadClientProvider.GetDownloadClients();

            foreach (var client in clients)
            {
                try
                {
                    var status = client.GetStatus();
                    var folders = status.OutputRootFolders;

                    foreach (var folder in folders)
                    {
                        if (!folder.IsValid)
                        {
                            if (!status.IsLocalhost)
                            {
                                return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format("Remote download client {0} places downloads in {1} but this is not a valid {2} path. Review your remote path mappings and download client settings.", client.Definition.Name, folder.FullPath, _osInfo.Name), "#bad-remote-path-mapping");
                            }
                            else if (_osInfo.IsDocker)
                            {
                                return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format("You are using docker; download client {0} reported files in {1} but this is not a valid {2} path. Review your remote path mappings and download client settings.", client.Definition.Name, folder.FullPath, _osInfo.Name), "#docker-bad-remote-path-mapping");
                            }
                            else
                            {
                                return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format("Local download client {0} places downloads in {1} but this is not a valid {2} path. Review your download client settings.", client.Definition.Name, folder.FullPath, _osInfo.Name), "#bad-download-client-settings");
                            }
                        }

                        if (!_diskProvider.FolderExists(folder.FullPath))
                        {
                            if (_osInfo.IsDocker)
                            {
                                return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format("You are using docker; download client {0} places downloads in {1} but this directory does not appear to exist inside the container. Review your remote path mappings and container volume settings.", client.Definition.Name, folder.FullPath), "#docker-bad-remote-path-mapping");
                            }
                            else if (!status.IsLocalhost)
                            {
                                return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format("Remote download client {0} places downloads in {1} but this directory does not appear to exist. Likely missing or incorrect remote path mapping.", client.Definition.Name, folder.FullPath), "#bad-remote-path-mapping");
                            }
                            else
                            {
                                return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format("Download client {0} places downloads in {1} but Sonarr cannot see this directory. You may need to adjust the folder's permissions.", client.Definition.Name, folder.FullPath), "#permissions-error");
                            }
                        }
                    }
                }
                catch (DownloadClientException ex)
                {
                    _logger.Debug(ex, "Unable to communicate with {0}", client.Definition.Name);
                }
                catch (HttpRequestException ex)
                {
                    _logger.Debug(ex, "Unable to communicate with {0}", client.Definition.Name);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Unknown error occured in RemotePathMapping HealthCheck");
                }
            }

            return new HealthCheck(GetType());
        }

        public HealthCheck Check(IEvent message)
        {
            // We don't care about client folders if we are not handling completed files
            if (!_configService.EnableCompletedDownloadHandling)
            {
                return new HealthCheck(GetType());
            }

            if (typeof(EpisodeImportFailedEvent).IsAssignableFrom(message.GetType()))
            {
                var failureMessage = (EpisodeImportFailedEvent)message;

                // if we can see the file exists but the import failed then likely a permissions issue
                if (failureMessage.EpisodeInfo != null)
                {
                    var episodePath = failureMessage.EpisodeInfo.Path;

                    if (_diskProvider.FileExists(episodePath))
                    {
                        return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format("Sonarr can see but not access downloaded episode {0}. Likely permissions error.", episodePath), "#permissions-error");
                    }
                    else
                    {
                        // If the file doesn't exist but EpisodeInfo is not null then the message is coming from
                        // ImportApprovedEpisodes and the file must have been removed part way through processing
                        return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format("File {0} was removed part way through processing.", episodePath), "#remote-path-file-removed");
                    }
                }

                // If the previous case did not match then the failure occured in DownloadedEpisodeImportService,
                // while trying to locate the files reported by the download client
                // Only check clients not in failure status, those get another message
                var client = _downloadClientProvider.GetDownloadClients().FirstOrDefault(x => x.Definition.Name == failureMessage.DownloadClientInfo.Name);

                if (client == null)
                {
                    return new HealthCheck(GetType());
                }

                try
                {
                    var status = client.GetStatus();
                    var dlpath = client?.GetItems().FirstOrDefault(x => x.DownloadId == failureMessage.DownloadId)?.OutputPath.FullPath;

                    // If dlpath is null then there's not much useful we can report. Give a generic message so
                    // that the user realises something is wrong.
                    if (dlpath.IsNullOrWhiteSpace())
                    {
                        return new HealthCheck(GetType(), HealthCheckResult.Error, "Sonarr failed to import (an) episode(s). Check your logs for details.", "#remote-path-import-failed");
                    }

                    if (!dlpath.IsPathValid())
                    {
                        if (!status.IsLocalhost)
                        {
                            return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format("Remote download client {0} reported files in {1} but this is not a valid {2} path. Review your remote path mappings and download client settings.", client.Definition.Name, dlpath, _osInfo.Name), "#bad-remote-path-mapping");
                        }
                        else if (_osInfo.IsDocker)
                        {
                            return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format("You are using docker; download client {0} reported files in {1} but this is not a valid {2} path. Review your remote path mappings and download client settings.", client.Definition.Name, dlpath, _osInfo.Name), "#docker-bad-remote-path-mapping");
                        }
                        else
                        {
                            return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format("Local download client {0} reported files in {1} but this is not a valid {2} path. Review your download client settings.", client.Definition.Name, dlpath, _osInfo.Name), "#bad-download-client-settings");
                        }
                    }

                    if (_diskProvider.FolderExists(dlpath))
                    {
                        return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format("Sonarr can see but not access download directory {0}. Likely permissions error.", dlpath), "#permissions-error");
                    }

                    // if it's a remote client/docker, likely missing path mappings
                    if (_osInfo.IsDocker)
                    {
                        return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format("Sonarr can see but not access download directory {0}. Likely permissions error.", client.Definition.Name, dlpath), "#docker-bad-remote-path-mapping");
                    }
                    else if (!status.IsLocalhost)
                    {
                        return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format("Remote download client {0} reported files in {1} but this directory does not appear to exist. Likely missing remote path mapping.", client.Definition.Name, dlpath), "#bad-remote-path-mapping");
                    }
                    else
                    {
                        // path mappings shouldn't be needed locally so probably a permissions issue
                        return new HealthCheck(GetType(), HealthCheckResult.Error, string.Format("Download client {0} reported files in {1} but Sonarr cannot see this directory. You may need to adjust the folder's permissions.", client.Definition.Name, dlpath), "#permissions-error");
                    }
                }
                catch (DownloadClientException ex)
                {
                    _logger.Debug(ex, "Unable to communicate with {0}", client.Definition.Name);
                }
                catch (HttpRequestException ex)
                {
                    _logger.Debug(ex, "Unable to communicate with {0}", client.Definition.Name);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Unknown error occured in RemotePathMapping HealthCheck");
                }

                return new HealthCheck(GetType());
            }
            else
            {
                return Check();
            }
        }
    }
}
