using System.Net;
using NLog;
using NzbDrone.Common.Messaging;
using RestSharp;

namespace NzbDrone.Core.Notifications.Pushover
{
    public interface IPushoverService
    {
        void SendNotification(string title, string message, string userKey, PushoverPriority priority);
    }

    public class PushoverService : IPushoverService, IExecute<TestPushoverCommand>
    {
        private readonly Logger _logger;

        private const string TOKEN = "yz9b4U215iR4vrKFRfjNXP24NMNPKJ";
        private const string URL = "https://api.pushover.net/1/messages.json";

        public PushoverService(Logger logger)
        {
            _logger = logger;
        }

        public void SendNotification(string title, string message, string userKey, PushoverPriority priority)
        {
            var client = new RestClient(URL);
            var request = new RestRequest(Method.POST);
            request.AddParameter("token", TOKEN);
            request.AddParameter("user", userKey);
            request.AddParameter("title", title);
            request.AddParameter("message", message);
            request.AddParameter("priority", (int)priority);

            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidResponseException(response.Content);
            }
        }

        public void Execute(TestPushoverCommand message)
        {
            const string title = "Test Notification";
            const string body = "This is a test message from NzbDrone";

            SendNotification(title, body, message.UserKey, (PushoverPriority)message.Priority);
        }
    }
}
