using System;
using System.Collections.Generic;
using System.Linq;
using Growl.Connector;
using NLog;
using GrowlNotification = Growl.Connector.Notification;

namespace NzbDrone.Core.Notifications.Growl
{
    public class GrowlProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Application _growlApplication = new Application("NzbDrone");
        private GrowlConnector _growlConnector;
        private List<NotificationType> _notificationTypes;

        public GrowlProvider()
        {
            _notificationTypes = GetNotificationTypes();
            _growlApplication.Icon = "https://github.com/NzbDrone/NzbDrone/raw/master/NzbDrone.Core/NzbDrone.jpg";
        }

        public virtual void Register(string hostname, int port, string password)
        {
            Logger.Trace("Registering NzbDrone with Growl host: {0}:{1}", hostname, port);
            _growlConnector = new GrowlConnector(password, hostname, port);
            _growlConnector.Register(_growlApplication, _notificationTypes.ToArray());
        }

        public virtual void TestNotification(string hostname, int port, string password)
        {
            const string title = "Test Notification";
            const string message = "This is a test message from NzbDrone";

            SendNotification(title, message, "TEST", hostname, port, password);
        }

        public virtual void SendNotification(string title, string message, string notificationTypeName, string hostname, int port, string password)
        {
            var notificationType = _notificationTypes.Single(n => n.Name == notificationTypeName);

            var notification = new GrowlNotification("NzbDrone", notificationType.Name, DateTime.Now.Ticks.ToString(), title, message);

            _growlConnector = new GrowlConnector(password, hostname, port);

            Logger.Trace("Sending Notification to: {0}:{1}", hostname, port);
            _growlConnector.Notify(notification);
        }

        private List<NotificationType> GetNotificationTypes()
        {
            var notificationTypes = new List<NotificationType>();
            notificationTypes.Add(new NotificationType("TEST", "Test"));
            notificationTypes.Add(new NotificationType("GRAB", "Episode Grabbed"));
            notificationTypes.Add(new NotificationType("DOWNLOAD", "Episode Complete"));

            return notificationTypes;
        }
    }
}
