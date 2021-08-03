using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.HealthCheck;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Events;
using NzbDrone.Core.Update.History.Events;

namespace NzbDrone.Core.Notifications
{
    public class NotificationService
        : IHandle<EpisodeGrabbedEvent>,
          IHandle<EpisodeImportedEvent>,
          IHandle<SeriesRenamedEvent>,
          IHandle<SeriesDeletedEvent>,
          IHandle<EpisodeFileDeletedEvent>,
          IHandle<HealthCheckFailedEvent>,
          IHandle<UpdateInstalledEvent>,
          IHandleAsync<DeleteCompletedEvent>,
          IHandleAsync<DownloadsProcessedEvent>,
          IHandleAsync<RenameCompletedEvent>,
          IHandleAsync<HealthCheckCompleteEvent>
    {
        private readonly INotificationFactory _notificationFactory;
        private readonly Logger _logger;

        public NotificationService(INotificationFactory notificationFactory, Logger logger)
        {
            _notificationFactory = notificationFactory;
            _logger = logger;
        }

        private string GetMessage(Series series, List<Episode> episodes, QualityModel quality)
        {
            var qualityString = quality.Quality.ToString();

            if (quality.Revision.Version > 1)
            {
                if (series.SeriesType == SeriesTypes.Anime)
                {
                    qualityString += " v" + quality.Revision.Version;
                }
                else
                {
                    qualityString += " Proper";
                }
            }

            if (series.SeriesType == SeriesTypes.Daily)
            {
                var episode = episodes.First();

                return string.Format("{0} - {1} - {2} [{3}]",
                                         series.Title,
                                         episode.AirDate,
                                         episode.Title,
                                         qualityString);
            }

            var episodeNumbers = string.Concat(episodes.Select(e => e.EpisodeNumber)
                                                       .Select(i => string.Format("x{0:00}", i)));

            var episodeTitles = string.Join(" + ", episodes.Select(e => e.Title));

            return string.Format("{0} - {1}{2} - {3} [{4}]",
                                    series.Title,
                                    episodes.First().SeasonNumber,
                                    episodeNumbers,
                                    episodeTitles,
                                    qualityString);
        }

        private bool ShouldHandleSeries(ProviderDefinition definition, Series series)
        {
            if (definition.Tags.Empty())
            {
                _logger.Debug("No tags set for this notification.");
                return true;
            }

            if (definition.Tags.Intersect(series.Tags).Any())
            {
                _logger.Debug("Notification and series have one or more intersecting tags.");
                return true;
            }

            _logger.Debug("{0} does not have any intersecting tags with {1}. Notification will not be sent.", definition.Name, series.Title);
            return false;
        }

        private bool ShouldHandleHealthFailure(HealthCheck.HealthCheck healthCheck, bool includeWarnings)
        {
            if (healthCheck.Type == HealthCheckResult.Error)
            {
                return true;
            }

            if (healthCheck.Type == HealthCheckResult.Warning && includeWarnings)
            {
                return true;
            }

            return false;
        }

        public void Handle(EpisodeGrabbedEvent message)
        {
            var grabMessage = new GrabMessage
            {
                Message = GetMessage(message.Episode.Series, message.Episode.Episodes, message.Episode.ParsedEpisodeInfo.Quality),
                Series = message.Episode.Series,
                Quality = message.Episode.ParsedEpisodeInfo.Quality,
                Episode = message.Episode,
                DownloadClientType = message.DownloadClient,
                DownloadClientName = message.DownloadClientName,
                DownloadId = message.DownloadId
            };

            foreach (var notification in _notificationFactory.OnGrabEnabled())
            {
                try
                {
                    if (!ShouldHandleSeries(notification.Definition, message.Episode.Series))
                    {
                        continue;
                    }

                    notification.OnGrab(grabMessage);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Unable to send OnGrab notification to {0}", notification.Definition.Name);
                }
            }
        }

        public void Handle(EpisodeImportedEvent message)
        {
            if (!message.NewDownload)
            {
                return;
            }

            var downloadMessage = new DownloadMessage
            {
                Message = GetMessage(message.EpisodeInfo.Series, message.EpisodeInfo.Episodes, message.EpisodeInfo.Quality),
                Series = message.EpisodeInfo.Series,
                EpisodeFile = message.ImportedEpisode,
                OldFiles = message.OldFiles,
                SourcePath = message.EpisodeInfo.Path,
                DownloadClientInfo = message.DownloadClientInfo,
                DownloadId = message.DownloadId
            };

            foreach (var notification in _notificationFactory.OnDownloadEnabled())
            {
                try
                {
                    if (ShouldHandleSeries(notification.Definition, message.EpisodeInfo.Series))
                    {
                        if (downloadMessage.OldFiles.Empty() || ((NotificationDefinition)notification.Definition).OnUpgrade)
                        {
                            notification.OnDownload(downloadMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Unable to send OnDownload notification to: " + notification.Definition.Name);
                }
            }
        }

        public void Handle(SeriesRenamedEvent message)
        {
            foreach (var notification in _notificationFactory.OnRenameEnabled())
            {
                try
                {
                    if (ShouldHandleSeries(notification.Definition, message.Series))
                    {
                        notification.OnRename(message.Series, message.RenamedFiles);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Unable to send OnRename notification to: " + notification.Definition.Name);
                }
            }
        }

        public void Handle(UpdateInstalledEvent message)
        {
            var updateMessage = new ApplicationUpdateMessage();
            updateMessage.Message = $"Sonarr updated from {message.PreviousVerison.ToString()} to {message.NewVersion.ToString()}";
            updateMessage.PreviousVersion = message.PreviousVerison;
            updateMessage.NewVersion = message.NewVersion;

            foreach (var notification in _notificationFactory.OnApplicationUpdateEnabled())
            {
                try
                {
                    notification.OnApplicationUpdate(updateMessage);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Unable to send OnApplicationUpdate notification to: " + notification.Definition.Name);
                }
            }
        }

        public void Handle(EpisodeFileDeletedEvent message)
        {
            if (message.EpisodeFile.Episodes.Value.Empty())
            {
                _logger.Trace("Skipping notification for deleted file without an episode (episode metadata was removed)");

                return;
            }

            var deleteMessage = new EpisodeDeleteMessage();
            deleteMessage.Message = GetMessage(message.EpisodeFile.Series, message.EpisodeFile.Episodes, message.EpisodeFile.Quality);
            deleteMessage.Series = message.EpisodeFile.Series;
            deleteMessage.EpisodeFile = message.EpisodeFile;
            deleteMessage.Reason = message.Reason;

            foreach (var notification in _notificationFactory.OnEpisodeFileDeleteEnabled())
            {
                try
                {
                    if (message.Reason != MediaFiles.DeleteMediaFileReason.Upgrade || ((NotificationDefinition)notification.Definition).OnEpisodeFileDeleteForUpgrade)
                    {
                        if (ShouldHandleSeries(notification.Definition, deleteMessage.EpisodeFile.Series))
                        {
                            notification.OnEpisodeFileDelete(deleteMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Unable to send OnDelete notification to: " + notification.Definition.Name);
                }
            }
        }

        public void Handle(SeriesDeletedEvent message)
        {
            var deleteMessage = new SeriesDeleteMessage(message.Series, message.DeleteFiles);

            foreach (var notification in _notificationFactory.OnSeriesDeleteEnabled())
            {
                try
                {
                    if (ShouldHandleSeries(notification.Definition, deleteMessage.Series))
                    {
                        notification.OnSeriesDelete(deleteMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Unable to send OnDelete notification to: " + notification.Definition.Name);
                }
            }
        }

        public void Handle(HealthCheckFailedEvent message)
        {
            // Don't send health check notifications during the start up grace period,
            // once that duration expires they they'll be retested and fired off if necessary.

            if (message.IsInStartupGraceperiod)
            {
                return;
            }

            foreach (var notification in _notificationFactory.OnHealthIssueEnabled())
            {
                try
                {
                    if (ShouldHandleHealthFailure(message.HealthCheck, ((NotificationDefinition)notification.Definition).IncludeHealthWarnings))
                    {
                        notification.OnHealthIssue(message.HealthCheck);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Unable to send OnHealthIssue notification to: " + notification.Definition.Name);
                }
            }
        }

        public void HandleAsync(DeleteCompletedEvent message)
        {
            ProcessQueue();
        }

        public void HandleAsync(DownloadsProcessedEvent message)
        {
            ProcessQueue();
        }

        public void HandleAsync(RenameCompletedEvent message)
        {
            ProcessQueue();
        }

        public void HandleAsync(HealthCheckCompleteEvent message)
        {
            ProcessQueue();
        }

        private void ProcessQueue()
        {
            foreach (var notification in _notificationFactory.GetAvailableProviders())
            {
                try
                {
                    notification.ProcessQueue();
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Unable to process notification queue for " + notification.Definition.Name);
                }
            }
        }
    }
}
