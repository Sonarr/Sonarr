using System;
using NzbDrone.Core.Messaging.Commands;
using RestSharp;
using NzbDrone.Core.Rest;

namespace NzbDrone.Core.Notifications.PushBullet
{
    public interface IPushBulletProxy
    {
        void SendNotification(string title, string message, string apiKey, int deviceId);
    }

    public class PushBulletProxy : IPushBulletProxy, IExecute<TestPushBulletCommand>
    {
        private const string URL = "https://www.pushbullet.com/api/pushes";

        public void SendNotification(string title, string message, string apiKey, int deviceId)
        {
            var client = new RestClient(URL);
            var request = new RestRequest(Method.POST);
            request.AddParameter("device_id", deviceId);
            request.AddParameter("type", "note");
            request.AddParameter("title", title);
            request.AddParameter("body", message);

            client.Authenticator = new HttpBasicAuthenticator(apiKey, String.Empty);
            client.ExecuteAndValidate(request);
        }

        public void Execute(TestPushBulletCommand message)
        {
            const string title = "Test Notification";
            const string body = "This is a test message from NzbDrone";

            SendNotification(title, body, message.ApiKey, message.DeviceId);
        }
    }
}
