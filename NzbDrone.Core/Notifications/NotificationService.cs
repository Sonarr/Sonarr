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
    public interface INotificationService
    {
        List<Notification> All();
        Notification Get(int id);
        List<Notification> Schema();
        Notification Create(Notification notification);
        Notification Update(Notification notification);
        void Delete(int id);
    }

    public class NotificationService
        : INotificationService,
          IHandle<EpisodeGrabbedEvent>,
          IHandle<EpisodeDownloadedEvent>,
          IHandle<SeriesRenamedEvent>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IContainer _container;
        private readonly List<INotification> _notifications;
        private readonly Logger _logger;

        public NotificationService(INotificationRepository notificationRepository,
                                   IEnumerable<INotification> notifications,
                                   IContainer container,
                                   Logger logger)
        {
            _notificationRepository = notificationRepository;
            _container = container;
            _notifications = notifications.ToList();
            _logger = logger;
        }

        public List<Notification> All()
        {
            return _notificationRepository.All().Select(ToNotification).ToList();
        }

        public Notification Get(int id)
        {
            return ToNotification(_notificationRepository.Get(id));
        }

        public List<Notification> Schema()
        {
            var notifications = new List<Notification>();

            int i = 1;
            foreach (var notification in _notifications)
            {
                var type = notification.GetType();

                var newNotification = new Notification();
                newNotification.Instance = (INotification)_container.Resolve(type);
                newNotification.Id = i;
                newNotification.ImplementationName = notification.ImplementationName;
                newNotification.Link = notification.Link;

                var instanceType = newNotification.Instance.GetType();
                var baseGenArgs = instanceType.BaseType.GetGenericArguments();
                newNotification.Settings = (IProviderConfig)Activator.CreateInstance(baseGenArgs[0]);
                newNotification.Implementation = type.Name;

                notifications.Add(newNotification);
                i++;
            }

            return notifications.OrderBy(n => n.Name).ToList();
        }

        public Notification Create(Notification notification)
        {
            var definition = new NotificationDefinition();
            definition.InjectFrom(notification);
            definition.Settings = notification.Settings.ToJson();

            definition = _notificationRepository.Insert(definition);
            notification.Id = definition.Id;

            return notification;
        }

        public Notification Update(Notification notification)
        {
            var definition = _notificationRepository.Get(notification.Id);
            definition.InjectFrom(notification);
            definition.Settings = notification.Settings.ToJson();

            _notificationRepository.Update(definition);

            return notification;
        }

        public void Delete(int id)
        {
            _notificationRepository.Delete(id);
        }

        private Notification ToNotification(NotificationDefinition definition)
        {
            var notification = new Notification();
            notification.Id = definition.Id;
            notification.OnGrab = definition.OnGrab;
            notification.OnDownload = definition.OnDownload;
            notification.Instance = GetInstance(definition);
            notification.Name = definition.Name;
            notification.Implementation = definition.Implementation;
            notification.ImplementationName = notification.Instance.ImplementationName;
            notification.Settings = ((dynamic)notification.Instance).ImportSettingsFromJson(definition.Settings);

            return notification;
        }

        private INotification GetInstance(NotificationDefinition indexerDefinition)
        {           
            var type = _notifications.Single(c => c.GetType().Name.Equals(indexerDefinition.Implementation, StringComparison.InvariantCultureIgnoreCase)).GetType();

            var instance = (INotification)_container.Resolve(type);

            instance.InstanceDefinition = indexerDefinition;
            return instance;
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

            foreach (var notification in All().Where(n => n.OnGrab))
            {
                try
                {
                    notification.Instance.OnGrab(messageBody);
                }

                catch (Exception ex)
                {
                    _logger.ErrorException("Unable to send OnGrab notification to: " + notification.Name, ex);
                }
            }
        }

        public void Handle(EpisodeDownloadedEvent message)
        {
            var messageBody = GetMessage(message.Episode.Series, message.Episode.Episodes, message.Episode.ParsedEpisodeInfo.Quality);

            foreach (var notification in All().Where(n => n.OnDownload))
            {
                try
                {
                    notification.Instance.OnDownload(messageBody, message.Episode.Series);
                }

                catch (Exception ex)
                {
                    _logger.WarnException("Unable to send OnDownload notification to: " + notification.Name, ex);
                }
            }
        }

        public void Handle(SeriesRenamedEvent message)
        {
            foreach (var notification in All().Where(n => n.OnDownload))
            {
                try
                {
                    notification.Instance.AfterRename(message.Series);
                }

                catch (Exception ex)
                {
                    _logger.WarnException("Unable to send AfterRename notification to: " + notification.Name, ex);
                }
            }
        }
    }
}
