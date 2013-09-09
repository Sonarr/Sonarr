using System;
using System.Collections.Generic;
using System.Linq;
using Growl.Connector;
using NLog;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Common.Messaging;
using GrowlNotification = Growl.Connector.Notification;

namespace NzbDrone.Core.Notifications.Growl
{
    public interface IGrowlService
    {
        void SendNotification(string title, string message, string notificationTypeName, string hostname, int port, string password);
    }

    public class GrowlService : IGrowlService, IExecute<TestGrowlCommand>
    {
        private static readonly Logger Logger =  NzbDroneLogger.GetLogger();

        private readonly Application _growlApplication = new Application("NzbDrone");
        private GrowlConnector _growlConnector;
        private readonly List<NotificationType> _notificationTypes;

        public GrowlService()
        {
            _notificationTypes = GetNotificationTypes();
            _growlApplication.Icon = "https://raw.github.com/NzbDrone/NzbDrone/master/Logo/64.png";
        }

        public void SendNotification(string title, string message, string notificationTypeName, string hostname, int port, string password)
        {
            var notificationType = _notificationTypes.Single(n => n.Name == notificationTypeName);
            var notification = new GrowlNotification("NzbDrone", notificationType.Name, DateTime.Now.Ticks.ToString(), title, message);

            _growlConnector = new GrowlConnector(password, hostname, port);

            Logger.Trace("Sending Notification to: {0}:{1}", hostname, port);
            _growlConnector.Notify(notification);
        }

        private void Register(string host, int port, string password)
        {
            Logger.Trace("Registering NzbDrone with Growl host: {0}:{1}", host, port);
            _growlConnector = new GrowlConnector(password, host, port);
            _growlConnector.Register(_growlApplication, _notificationTypes.ToArray());
        }

        private List<NotificationType> GetNotificationTypes()
        {
            var notificationTypes = new List<NotificationType>();
            notificationTypes.Add(new NotificationType("TEST", "Test"));
            notificationTypes.Add(new NotificationType("GRAB", "Episode Grabbed"));
            notificationTypes.Add(new NotificationType("DOWNLOAD", "Episode Complete"));

            return notificationTypes;
        }

        public void Execute(TestGrowlCommand message)
        {
            Register(message.Host, message.Port, message.Password);

            const string title = "Test Notification";
            const string body = "This is a test message from NzbDrone";

            SendNotification(title, body, "TEST", message.Host, message.Port, message.Password);
        }
    }
}
