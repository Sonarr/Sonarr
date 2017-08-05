using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;

namespace NzbDrone.SignalR
{
    public interface IBroadcastSignalRMessage
    {
        void BroadcastMessage(SignalRMessage message);
    }

    public sealed class NzbDronePersistentConnection : PersistentConnection, IBroadcastSignalRMessage
    {
        private IPersistentConnectionContext Context => ((ConnectionManager)GlobalHost.ConnectionManager).GetConnection(GetType());

        private static string API_KEY;

        public NzbDronePersistentConnection(IConfigFileProvider configFileProvider)
        {
            API_KEY = configFileProvider.ApiKey;
        }

        public void BroadcastMessage(SignalRMessage message)
        {
            Context.Connection.Broadcast(message);
        }

        protected override bool AuthorizeRequest(IRequest request)
        {
            var apiKey = request.QueryString["apiKey"];

            if (apiKey.IsNotNullOrWhiteSpace() && apiKey.Equals(API_KEY))
            {
                return true;
            }

            return false;
        }
    }
}