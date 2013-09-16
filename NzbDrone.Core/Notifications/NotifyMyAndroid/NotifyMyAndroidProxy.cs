using System;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using NLog.LayoutRenderers;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Commands;
using RestSharp;
using NzbDrone.Core.Rest;

namespace NzbDrone.Core.Notifications.NotifyMyAndroid
{
    public interface INotifyMyAndroidProxy
    {
        void SendNotification(string title, string message, string apiKye, NotifyMyAndroidPriority priority);
    }

    public class NotifyMyAndroidProxy : INotifyMyAndroidProxy, IExecute<TestNotifyMyAndroidCommand>
    {
        private const string URL = "https://www.notifymyandroid.com/publicapi";

        public void SendNotification(string title, string message, string apiKey, NotifyMyAndroidPriority priority)
        {
            var client = new RestClient(URL);
            var request = new RestRequest("notify", Method.POST);
            request.RequestFormat = DataFormat.Xml;
            request.AddParameter("apikey", apiKey);
            request.AddParameter("application", "NzbDrone");
            request.AddParameter("event", title);
            request.AddParameter("description", message);
            request.AddParameter("priority", (int)priority);

            var response = client.ExecuteAndValidate(request);
            ValidateResponse(response);
        }

        private void Verify(string apiKey)
        {
            var client = new RestClient(URL);
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

        public void Execute(TestNotifyMyAndroidCommand message)
        {
            const string title = "Test Notification";
            const string body = "This is a test message from NzbDrone";
            Verify(message.ApiKey);
            SendNotification(title, body, message.ApiKey, (NotifyMyAndroidPriority)message.Priority);
        }
    }
}
