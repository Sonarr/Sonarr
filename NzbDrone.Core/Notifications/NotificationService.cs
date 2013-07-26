using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Composition;
using NzbDrone.Common.Messaging;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Parser.Model;
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

                var instanceType = newNotification.Instance.GetType();
                var baseGenArgs = instanceType.BaseType.GetGenericArguments();
                newNotification.Settings = (INotifcationSettings)Activator.CreateInstance(baseGenArgs[0]);
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

        private string GetMessage(ParsedEpisodeInfo parsedEpisodeInfo, Series series)
        {
            if (series.SeriesType == SeriesTypes.Daily)
            {
                return String.Format("{0} - {1}",
                                 series.Title,
                                 parsedEpisodeInfo.AirDate.Value.ToString(Episode.AIR_DATE_FORMAT));
            }
            
            return String.Format("{0} - {1}{2}",
                                 series.Title,
                                 parsedEpisodeInfo.SeasonNumber,
                                 String.Concat(parsedEpisodeInfo.EpisodeNumbers.Select(i => String.Format("x{0:00}", i))));
        }

        public void Handle(EpisodeGrabbedEvent message)
        {
            var messageBody = GetMessage(message.Episode.ParsedEpisodeInfo, message.Episode.Series);

            All().Where(n => n.OnGrab)
                .ToList()
                .ForEach(notification =>
                            notification.Instance
                                        .OnGrab(messageBody)
                        );
        }

        public void Handle(EpisodeDownloadedEvent message)
        {
            var messageBody = GetMessage(message.ParsedEpisodeInfo, message.Series);

            All().Where(n => n.OnDownload)
                .ToList()
                .ForEach(notification =>
                            notification.Instance
                                        .OnDownload(messageBody, message.Series)
                        );
        }

        public void Handle(SeriesRenamedEvent message)
        {
            All().Where(n => n.OnDownload)
                .ToList()
                .ForEach(notification =>
                            notification.Instance
                                        .AfterRename(message.Series)
                        );
        }
    }
}
