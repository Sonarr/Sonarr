using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Composition;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;
using Omu.ValueInjecter;

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
            if (series.SeriesType == SeriesTypes.Daily)
            {
                var episode = episodes.First();

                return String.Format("{0} - {1} - {2} [{3}]",
                                         series.Title,
                                         episode.AirDate,
                                         episode.Title,
                                         quality);
            }

            var episodeNumbers = String.Concat(episodes.Select(e => e.EpisodeNumber)
                                                       .Select(i => String.Format("x{0:00}", i)));

            var episodeTitles = String.Join(" + ", episodes.Select(e => e.Title));

            return String.Format("{0} - {1}{2} - {3} {4}",
                                    series.Title,
                                    episodes.First().SeasonNumber,
                                    episodeNumbers,
                                    episodeTitles,
                                    quality);
        }

        public void Handle(EpisodeGrabbedEvent message)
        {
            var messageBody = GetMessage(message.Episode.Series, message.Episode.Episodes, message.Episode.ParsedEpisodeInfo.Quality);

            foreach (var notification in _notificationFactory.OnGrabEnabled())
            {
                try
                {
                    notification.OnGrab(messageBody);
                }

                catch (Exception ex)
                {
                    _logger.ErrorException("Unable to send OnGrab notification to: " + notification.Definition.Name, ex);
                }
            }
        }

        public void Handle(EpisodeDownloadedEvent message)
        {
            var downloadMessage = new DownloadMessage();
            downloadMessage.Message = GetMessage(message.Episode.Series, message.Episode.Episodes, message.Episode.ParsedEpisodeInfo.Quality);
            downloadMessage.Series = message.Episode.Series;
            downloadMessage.EpisodeFile = message.EpisodeFile;
            downloadMessage.OldFiles = message.OldFiles;

            foreach (var notification in _notificationFactory.OnDownloadEnabled())
            {
                try
                {
                    notification.OnDownload(downloadMessage);
                }

                catch (Exception ex)
                {
                    _logger.WarnException("Unable to send OnDownload notification to: " + notification.Definition.Name, ex);
                }
            }
        }

        public void Handle(SeriesRenamedEvent message)
        {
            foreach (var notification in _notificationFactory.OnDownloadEnabled())
            {
                try
                {
                    notification.AfterRename(message.Series);
                }

                catch (Exception ex)
                {
                    _logger.WarnException("Unable to send AfterRename notification to: " + notification.Definition.Name, ex);
                }
            }
        }
    }
}
