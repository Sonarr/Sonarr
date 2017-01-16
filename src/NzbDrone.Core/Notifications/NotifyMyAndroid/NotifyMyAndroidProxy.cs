using System;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Core.Exceptions;
using RestSharp;
using NzbDrone.Core.Rest;

namespace NzbDrone.Core.Notifications.NotifyMyAndroid
{
    public interface INotifyMyAndroidProxy
    {
        void SendNotification(string title, string message, string apiKye, NotifyMyAndroidPriority priority);
        ValidationFailure Test(NotifyMyAndroidSettings settings);
    }

    public class NotifyMyAndroidProxy : INotifyMyAndroidProxy
    {
        private readonly Logger _logger;
        private const string URL = "https://www.notifymyandroid.com/publicapi";

        public NotifyMyAndroidProxy(Logger logger)
        {
            _logger = logger;
        }

        public void SendNotification(string title, string message, string apiKey, NotifyMyAndroidPriority priority)
        {
            var client = RestClientFactory.BuildClient(URL);
            var request = new RestRequest("notify", Method.POST);
            request.RequestFormat = DataFormat.Xml;
            request.AddParameter("apikey", apiKey);
            request.AddParameter("application", "Sonarr");
            request.AddParameter("event", title);
            request.AddParameter("description", message);
            request.AddParameter("priority", (int)priority);

            var response = client.ExecuteAndValidate(request);
            ValidateResponse(response);
        }

        private void Verify(string apiKey)
        {
            var client = RestClientFactory.BuildClient(URL);
            var request = new RestRequest("verify", Method.GET);
            request.RequestFormat = DataFormat.Xml;
            request.AddParameter("apikey", apiKey, ParameterType.GetOrPost);

            var response = client.ExecuteAndValidate(request);
            ValidateResponse(response);
        }

        private void ValidateResponse(IRestResponse response)
        {
            var xDoc = XDocument.Parse(response.Content);
            var nma = xDoc.Descendants("nma").Single();
            var error = nma.Descendants("error").SingleOrDefault();

            if (error != null)
            {
                ((HttpStatusCode)Convert.ToInt32(error.Attribute("code").Value)).VerifyStatusCode(error.Value);
            }
        }

        public ValidationFailure Test(NotifyMyAndroidSettings settings)
        {
            try
            {
                const string title = "Test Notification";
                const string body = "This is a test message from Sonarr";
                Verify(settings.ApiKey);
                SendNotification(title, body, settings.ApiKey, (NotifyMyAndroidPriority)settings.Priority);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("ApiKey", "Unable to send test message");
            }

            return null;
        }
    }
}
