using FluentValidation.Results;
using Growl.Connector;
using Growl.CoreLibrary;
using NzbDrone.Common.Extensions;
using GrowlNotification = Growl.Connector.Notification;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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

        //TODO: Change this to Sonarr, but it is a breaking change (v3)
        private readonly Application _growlApplication = new Application("NzbDrone");
        private readonly NotificationType[] _notificationTypes;

        private class GrowlRequestState
        {
            private AutoResetEvent _autoEvent = new AutoResetEvent(false);
            private bool _isError;
            private int _code;
            private string _description;

            public void Wait(int timeoutMs)
            {
                try
                {
                    if (!_autoEvent.WaitOne(timeoutMs))
                    {
                        throw new GrowlException(ErrorCode.TIMED_OUT, ErrorDescription.TIMED_OUT, null);
                    }
                    if (_isError)
                    {
                        throw new GrowlException(_code, _description, null);
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

            public void Update()
            {
                _autoEvent.Set();
            }

            public void Update(int code, string description)
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

            var logo = typeof(GrowlService).Assembly.GetManifestResourceBytes("NzbDrone.Core.Resources.Logo.64.png");

            _growlApplication.Icon = new BinaryData(logo);
        }

        private GrowlConnector GetGrowlConnector(string hostname, int port, string password)
        {
            var growlConnector = new GrowlConnector(password, hostname, port);
            growlConnector.OKResponse += GrowlOKResponse;
            growlConnector.ErrorResponse += GrowlErrorResponse;
            return growlConnector;
        }

        public void SendNotification(string title, string message, string notificationTypeName, string hostname, int port, string password)
        {
            _logger.Debug("Sending Notification to: {0}:{1}", hostname, port);

            var notificationType = _notificationTypes.Single(n => n.Name == notificationTypeName);
            var notification = new GrowlNotification(_growlApplication.Name, notificationType.Name, DateTime.Now.Ticks.ToString(), title, message);

            var growlConnector = GetGrowlConnector(hostname, port, password);

            var requestState = new GrowlRequestState();
            growlConnector.Notify(notification, requestState);
            requestState.Wait(5000);
        }

        private void Register(string host, int port, string password)
        {
            _logger.Debug("Registering Sonarr with Growl host: {0}:{1}", host, port);

            var growlConnector = GetGrowlConnector(host, port, password);

            var requestState = new GrowlRequestState();
            growlConnector.Register(_growlApplication, _notificationTypes, requestState);
            requestState.Wait(5000);
        }

        private void GrowlErrorResponse(Response response, object state)
        {
            var requestState = state as GrowlRequestState;
            if (requestState != null)
            {
                requestState.Update(response.ErrorCode, response.ErrorDescription);
            }
        }

        private void GrowlOKResponse(Response response, object state)
        {
            var requestState = state as GrowlRequestState;
            if (requestState != null)
            {
                requestState.Update();
            }
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
                const string body = "This is a test message from Sonarr";

                SendNotification(title, body, "TEST", settings.Host, settings.Port, settings.Password);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("Host", "Unable to send test message");
            }

            return null;
        }
    }
}
