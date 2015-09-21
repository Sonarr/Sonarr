using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications
{
    public class NotificationService
        : IHandle<EpisodeGrabbedEvent>,
          IHandle<EpisodeDownloadedEvent>,
          IHandle<SeriesRenamedEvent>,
          IHandle<MovieGrabbedEvent>,
          IHandle<MovieDownloadedEvent>,
          IHandle<MovieRenamedEvent>
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

                return String.Format("{0} - {1} - {2} [{3}]",
                                         series.Title,
                                         episode.AirDate,
                                         episode.Title,
                                         qualityString);
            }

            var episodeNumbers = String.Concat(episodes.Select(e => e.EpisodeNumber)
                                                       .Select(i => String.Format("x{0:00}", i)));

            var episodeTitles = String.Join(" + ", episodes.Select(e => e.Title));

            return String.Format("{0} - {1}{2} - {3} [{4}]",
                                    series.Title,
                                    episodes.First().SeasonNumber,
                                    episodeNumbers,
                                    episodeTitles,
                                    qualityString);
        }

        private string GetMessage(Movie movie, QualityModel quality)
        {
            var qualityString = quality.Quality.ToString();

            if (quality.Revision.Version > 1)
                qualityString += " Proper";


            return String.Format("{0} - [{1}]",
                                    movie.Title,
                                    qualityString);
        }


        private bool ShouldHandle(ProviderDefinition definition, HashSet<int> tags)
        {
            var notificationDefinition = (NotificationDefinition)definition;

            if (notificationDefinition.Tags.Empty())
            {
                _logger.Debug("No tags set for this notification.");
                return true;
            }

            if (notificationDefinition.Tags.Intersect(tags).Any())
            {
                _logger.Debug("Notification and series have one or more matching tags.");
                return true;
            }

            //TODO: this message could be more clear
            _logger.Debug("{0} does not have any maching tags", notificationDefinition.Name);
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
                    if (!ShouldHandle(notification.Definition, message.Episode.Series.Tags)) continue;
                    notification.OnGrab(grabMessage);
                }

                catch (Exception ex)
                {
                    _logger.ErrorException("Unable to send OnGrab notification to: " + notification.Definition.Name, ex);
                }
            }
        }

        public void Handle(MovieGrabbedEvent message)
        {

            var grabMessage = new GrabMovieMessage
            {
                Message = GetMessage(message.Movie.Movie, message.Movie.ParsedMovieInfo.Quality),
                Movie = message.Movie.Movie,
                RemoteMovie = message.Movie,
                Quality = message.Movie.ParsedMovieInfo.Quality
            };

            foreach (var notification in _notificationFactory.OnGrabMovieEnabled())
            {
                try
                {
                    if (!ShouldHandle(notification.Definition, message.Movie.Movie.Tags)) continue;
                    notification.OnGrabMovie(grabMessage);
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
            downloadMessage.Message = GetMessage(message.Episode.Series, message.Episode.Episodes, message.Episode.Quality);
            downloadMessage.Series = message.Episode.Series;
            downloadMessage.EpisodeFile = message.EpisodeFile;
            downloadMessage.OldFiles = message.OldFiles;
            downloadMessage.SourcePath = message.Episode.Path;

            foreach (var notification in _notificationFactory.OnDownloadEnabled())
            {
                try
                {
                    if (ShouldHandle(notification.Definition, message.Episode.Series.Tags))
                    {
                        if (downloadMessage.OldFiles.Empty() || ((NotificationDefinition)notification.Definition).OnUpgrade)
                        {
                            notification.OnDownload(downloadMessage);
                        }
                    }
                }

                catch (Exception ex)
                {
                    _logger.WarnException("Unable to send OnDownload notification to: " + notification.Definition.Name, ex);
                }
            }
        }

        public void Handle(MovieDownloadedEvent message)
        {
            var downloadMovieMessage = new DownloadMovieMessage();
            downloadMovieMessage.Message = GetMessage(message.Movie.Movie, message.Movie.Quality);
            downloadMovieMessage.Movie = message.Movie.Movie;
            downloadMovieMessage.MovieFile = message.MovieFile;
            downloadMovieMessage.OldFile = message.OldFile;
            downloadMovieMessage.SourcePath = message.Movie.Path;

            foreach (var notification in _notificationFactory.OnDownloadMovieEnabled())
            {
                try
                {
                    if (ShouldHandle(notification.Definition, message.Movie.Movie.Tags))
                    {
                        if (downloadMovieMessage.OldFile == null || ((NotificationDefinition)notification.Definition).OnUpgrade)
                        {
                            notification.OnDownloadMovie(downloadMovieMessage);
                        }
                    }
                }

                catch (Exception ex)
                {
                    _logger.WarnException("Unable to send OnDownloadMovie notification to: " + notification.Definition.Name, ex);
                }
            }
        }


        public void Handle(SeriesRenamedEvent message)
        {
            foreach (var notification in _notificationFactory.OnRenameEnabled())
            {
                try
                {
                    if (ShouldHandle(notification.Definition, message.Series.Tags))
                    {
                        notification.OnRename(message.Series);
                    }
                }

                catch (Exception ex)
                {
                    _logger.WarnException("Unable to send OnRename notification to: " + notification.Definition.Name, ex);
                }
            }
        }

        public void Handle(MovieRenamedEvent message)
        {
            foreach (var notification in _notificationFactory.OnRenameMovieEnabled())
            {
                try
                {
                    if (ShouldHandle(notification.Definition, message.Movie.Tags))
                    {
                        notification.OnRenameMovie(message.Movie);
                    }
                }

                catch (Exception ex)
                {
                    _logger.WarnException("Unable to send OnRenameMovie notification to: " + notification.Definition.Name, ex);
                }
            }
        }
    }
}
