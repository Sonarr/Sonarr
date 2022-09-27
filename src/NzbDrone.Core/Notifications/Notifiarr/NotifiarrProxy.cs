using System;
using System.Collections.Specialized;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Notifications.Notifiarr
{
    public interface INotifiarrProxy
    {
        void SendNotification(StringDictionary message, NotifiarrSettings settings);
        ValidationFailure Test(NotifiarrSettings settings);
    }

    public class NotifiarrProxy : INotifiarrProxy
    {
        private const string URL = "https://notifiarr.com";
        private readonly IHttpClient _httpClient;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly Logger _logger;

        public NotifiarrProxy(IHttpClient httpClient, IConfigFileProvider configFileProvider, Logger logger)
        {
            _httpClient = httpClient;
            _configFileProvider = configFileProvider;
            _logger = logger;
        }

        public void SendNotification(StringDictionary message, NotifiarrSettings settings)
        {
            try
            {
                ProcessNotification(message, settings);
            }
            catch (NotifiarrException ex)
            {
                throw ex;
            }
        }

        public ValidationFailure Test(NotifiarrSettings settings)
        {
            try
            {
                var variables = new StringDictionary();
                variables.Add("Sonarr_EventType", "Test");

                SendNotification(variables, settings);
                return null;
            }
            catch (NotifiarrException ex)
            {
                return new ValidationFailure("ApiKey", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return new ValidationFailure("", "Unable to send test notification. Check the log for more details.");
            }
        }

        private void ProcessNotification(StringDictionary message, NotifiarrSettings settings)
        {
            try
            {
                var instanceName = _configFileProvider.InstanceName;
                var requestBuilder = new HttpRequestBuilder(URL + "/api/v1/notification/sonarr").Post();
                requestBuilder.AddFormParameter("instanceName", instanceName).Build();
                requestBuilder.SetHeader("X-API-Key", settings.ApiKey);

                foreach (string key in message.Keys)
                {
                    requestBuilder.AddFormParameter(key, message[key]);
                }

                var request = requestBuilder.Build();

                _httpClient.Post(request);
            }
            catch (HttpException ex)
            {
                var responseCode = ex.Response.StatusCode;
                switch ((int)responseCode)
                {
                    case 401:
                        _logger.Error("Unauthorized", "HTTP 401 - API key is invalid");
                        throw new NotifiarrException("API key is invalid");
                    case 400:
                        _logger.Error("Invalid Request", "HTTP 400 - Unable to send notification. Ensure Sonarr Integration is enabled & assigned a channel on Notifiarr");
                        throw new NotifiarrException("Unable to send notification. Ensure Sonarr Integration is enabled & assigned a channel on Notifiarr");
                    case 502:
                    case 503:
                    case 504:
                        _logger.Error("Service Unavailable", "Unable to send notification. Service Unavailable");
                        throw new NotifiarrException("Unable to send notification. Service Unavailable", ex);
                    case 520:
                    case 521:
                    case 522:
                    case 523:
                    case 524:
                        _logger.Error(ex, "Cloudflare Related HTTP Error - Unable to send notification");
                        throw new NotifiarrException("Cloudflare Related HTTP Error - Unable to send notification", ex);
                    default:
                        _logger.Error(ex, "Unknown HTTP Error - Unable to send notification");
                        throw new NotifiarrException("Unknown HTTP Error - Unable to send notification", ex);
                }
            }
        }
    }
}
