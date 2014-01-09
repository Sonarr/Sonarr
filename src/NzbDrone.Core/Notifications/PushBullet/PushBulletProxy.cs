using System;
using NzbDrone.Core.Messaging.Commands;
using RestSharp;
using NzbDrone.Core.Rest;

namespace NzbDrone.Core.Notifications.PushBullet
{
    public interface IPushBulletProxy
    {
        void SendNotification(string title, string message, string apiKey, string deviceId);
    }

    public class PushBulletProxy : IPushBulletProxy, IExecute<TestPushBulletCommand>
    {
        private const string URL = "https://api.pushbullet.com/api/pushes";

        public void SendNotification(string title, string message, string apiKey, string deviceId)
        {
            var client = new RestClient(URL);
            var request = BuildRequest(deviceId); 
            
            request.AddParameter("type", "note");
            request.AddParameter("title", title);
            request.AddParameter("body", message);

            client.Authenticator = new HttpBasicAuthenticator(apiKey, String.Empty);
            client.ExecuteAndValidate(request);
        }

        public RestRequest BuildRequest(string deviceId)
        {
            var request = new RestRequest(Method.POST);
            long integerId;

            if (Int64.TryParse(deviceId, out integerId))
            {
                request.AddParameter("device_id", integerId);
            }

            else
            {
                request.AddParameter("device_iden", deviceId);
            }

            return request;
        }

        public void Execute(TestPushBulletCommand message)
        {
            const string title = "Test Notification";
            const string body = "This is a test message from NzbDrone";

            SendNotification(title, body, message.ApiKey, message.DeviceId);
        }
    }
}
