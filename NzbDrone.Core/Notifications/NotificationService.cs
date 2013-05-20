using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Composition;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Events;

namespace NzbDrone.Core.Notifications
{
    public interface INotificationService
    {
        List<Notification> All();
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
            var test = _notificationRepository.All();//.Select(ToNotification).ToList();
            return _notificationRepository.All().Select(ToNotification).ToList();
        }

        private Notification ToNotification(NotificationDefinition definition)
        {
            var notification = new Notification();
            notification.Id = definition.Id;
            notification.OnGrab = definition.OnGrab;
            notification.OnDownload = definition.OnDownload;
            notification.Instance = GetInstance(definition);
            notification.Name = definition.Name;
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

        public void Handle(EpisodeGrabbedEvent message)
        {
            All().Where(n => n.OnGrab)
                .ToList()
                .ForEach(notification =>
                            notification.Instance
                                        .OnGrab("Grabbed!")
                        );
        }

        public void Handle(EpisodeDownloadedEvent message)
        {
            All().Where(n => n.OnDownload)
                .ToList()
                .ForEach(notification =>
                            notification.Instance
                                        .OnDownload("Downloaded!", message.Series)
                        );
        }

        public void Handle(SeriesRenamedEvent message)
        {
            All().Where(n => n.OnDownload)
                .ToList()
                .ForEach(notification =>
                            notification.Instance
                                        .OnDownload("Renamed!", message.Series)
                        );
        }
    }
}
