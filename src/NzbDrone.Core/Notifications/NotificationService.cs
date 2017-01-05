using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications
{
    public class NotificationService
        : IHandle<EpisodeGrabbedEvent>,
          IHandle<EpisodeDownloadedEvent>,
          IHandle<SeriesRenamedEvent>
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
            var notificationDefinition = (NotificationDefinition)definition;

            if (notificationDefinition.Tags.Empty())
            {
                _logger.Debug("No tags set for this notification.");
                return true;
            }

            if (notificationDefinition.Tags.Intersect(series.Tags).Any())
            {
                _logger.Debug("Notification and series have one or more matching tags.");
                return true;
            }

            //TODO: this message could be more clear
            _logger.Debug("{0} does not have any tags that match {1}'s tags", notificationDefinition.Name, series.Title);
            return false;
        }

        public void Handle(EpisodeGrabbedEvent message)
        {
            var grabMessage = new GrabMessage
            {
                Message = GetMessage(message.Episode.Series, message.Episode.Episodes, message.Episode.ParsedEpisodeInfo.Quality),
                Series = message.Episode.Series,
                Quality = message.Episode.ParsedEpisodeInfo.Quality,
                Episode = message.Episode
            };

            foreach (var notification in _notificationFactory.OnGrabEnabled())
            {
                try
                {
                    if (!ShouldHandleSeries(notification.Definition, message.Episode.Series)) continue;
                    notification.OnGrab(grabMessage);
                }

                catch (Exception ex)
                {
                    _logger.Error(ex, "Unable to send OnGrab notification to {0}", notification.Definition.Name);
                }
            }
        }

        public void Handle(EpisodeDownloadedEvent message)
        {
            var downloadMessage = new DownloadMessage();
            downloadMessage.Message = GetMessage(message.Episode.Series, message.Episode.Episodes, message.Episode.Quality);
            downloadMessage.Series = message.Episode.Series;
            downloadMessage.EpisodeFile = message.EpisodeFile;
            downloadMessage.OldFiles = message.OldFiles;
            downloadMessage.SourcePath = message.Episode.Path;

            foreach (var notification in _notificationFactory.OnDownloadEnabled())
            {
                try
                {
                    if (ShouldHandleSeries(notification.Definition, message.Episode.Series))
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
                        notification.OnRename(message.Series);
                    }
                }

                catch (Exception ex)
                {
                    _logger.Warn(ex, "Unable to send OnRename notification to: " + notification.Definition.Name);
                }
            }
        }
    }
}
