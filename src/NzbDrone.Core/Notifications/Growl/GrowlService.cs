using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Drawing;
using FluentValidation.Results;
using Growl.CoreLibrary;
using Growl.Connector;
using NLog;
using NzbDrone.Common.Instrumentation;
using GrowlNotification = Growl.Connector.Notification;
using System.Reflection;
using System.IO;

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
        private readonly NotificationType[] _notificationTypes;

        private class GrowlResult
        {
            private AutoResetEvent _autoEvent = new AutoResetEvent(false);
            private bool _isError = false;
            private int _code;
            private string _description;

            public void Wait(int timeoutMs)
            {
                try
                {
                    if (!_autoEvent.WaitOne(timeoutMs))
                    {
                        throw new InvalidOperationException("timed out waiting for server");
                    }
                    if (_isError)
                    {
                        throw new InvalidOperationException(String.Format("{0}: {1}", _code, _description));
                    }
                }
                finally
                {
                    _autoEvent.Reset();
                    _isError = false;
                    _code = 0;
                    _description = null;
                }
            }

            public void Notify()
            {
                _autoEvent.Set();
            }

            public void Notify(int code, string description)
            {
                _code = code;
                _description = description;
                _isError = true;
                _autoEvent.Set();
            }
        }

        public GrowlService(Logger logger)
        {
            _logger = logger;
            _notificationTypes = GetNotificationTypes();
            var icon = NzbDrone.Core.Properties.Resources.growlIcon;
            var stream = new MemoryStream();
            icon.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
            _growlApplication.Icon = new BinaryData(stream.ToArray());
        }

        public void SendNotification(string title, string message, string notificationTypeName, string hostname, int port, string password)
        {
            _logger.Debug("Sending Notification to: {0}:{1}", hostname, port);
            var growlConnector = new GrowlConnector(password, hostname, port);
            growlConnector.OKResponse += GrowlOKResponse;
            growlConnector.ErrorResponse += GrowlErrorResponse;
            var result = new GrowlResult();
            var notificationType = _notificationTypes.Single(n => n.Name == notificationTypeName);
            var notification = new GrowlNotification(_growlApplication.Name, notificationType.Name, DateTime.Now.Ticks.ToString(), title, message);
            growlConnector.Notify(notification, result);
            result.Wait(10000);
        }

        private void Register(string host, int port, string password)
        {
            _logger.Debug("Registering NzbDrone with Growl host: {0}:{1}", host, port);
            var growlConnector = new GrowlConnector(password, host, port);
            growlConnector.OKResponse += GrowlOKResponse;
            growlConnector.ErrorResponse += GrowlErrorResponse;
            var result = new GrowlResult();
            growlConnector.Register(_growlApplication, _notificationTypes, result);
            result.Wait(20000);
        }

        private void GrowlErrorResponse(Response response, object state)
        {
            var result = state as GrowlResult;
            result.Notify(response.ErrorCode, response.ErrorDescription);
        }

        private void GrowlOKResponse(Response response, object state)
        {
            var result = state as GrowlResult;
            result.Notify();
        }

        private NotificationType[] GetNotificationTypes()
        {
            var notificationTypes = new List<NotificationType>();
            notificationTypes.Add(new NotificationType("TEST", "Test"));
            notificationTypes.Add(new NotificationType("GRAB", "Episode Grabbed"));
            notificationTypes.Add(new NotificationType("DOWNLOAD", "Episode Complete"));

            return notificationTypes.ToArray();
        }

        public ValidationFailure Test(GrowlSettings settings)
        {
            try
            {
                Register(settings.Host, settings.Port, settings.Password);

                const string title = "Test Notification";
                const string body = "This is a test message from NzbDrone";

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
