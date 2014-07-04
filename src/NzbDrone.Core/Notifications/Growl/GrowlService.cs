using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentValidation.Results;
using Growl.Connector;
using NLog;
using NzbDrone.Common.Instrumentation;
using GrowlNotification = Growl.Connector.Notification;

namespace NzbDrone.Core.Notifications.Growl
{
    public interface IGrowlService
    {
        void SendNotification(string title, string message, string notificationTypeName, string hostname, int port, string password);
        ValidationFailure Test(GrowlSettings settings);
    }

    public class GrowlService : IGrowlService
    {
        private readonly Logger _logger;

        private readonly Application _growlApplication = new Application("NzbDrone");
        private GrowlConnector _growlConnector;
        private readonly List<NotificationType> _notificationTypes;

        public GrowlService(Logger logger)
        {
            _logger = logger;
            _notificationTypes = GetNotificationTypes();
            _growlApplication.Icon = "https://raw.github.com/NzbDrone/NzbDrone/master/Logo/64.png";
        }

        public void SendNotification(string title, string message, string notificationTypeName, string hostname, int port, string password)
        {
            var notificationType = _notificationTypes.Single(n => n.Name == notificationTypeName);
            var notification = new GrowlNotification("NzbDrone", notificationType.Name, DateTime.Now.Ticks.ToString(), title, message);

            _growlConnector = new GrowlConnector(password, hostname, port);

            _logger.Debug("Sending Notification to: {0}:{1}", hostname, port);
            _growlConnector.Notify(notification);
        }

        private void Register(string host, int port, string password)
        {
            _logger.Debug("Registering NzbDrone with Growl host: {0}:{1}", host, port);
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

        public ValidationFailure Test(GrowlSettings settings)
        {
            try
            {
                Register(settings.Host, settings.Port, settings.Password);

                const string title = "Test Notification";
                const string body = "This is a test message from NzbDrone";

                Thread.Sleep(5000);

                SendNotification(title, body, "TEST", settings.Host, settings.Port, settings.Password);
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unable to send test message: " + ex.Message, ex);
                return new ValidationFailure("Host", "Unable to send test message");
            }

            return null;
        }
    }
}
