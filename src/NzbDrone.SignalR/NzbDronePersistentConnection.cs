using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Serializer;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore.Events;

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
        private readonly Dictionary<string, string> _messageHistory; 

        public NzbDronePersistentConnection(IConfigFileProvider configFileProvider)
        {
            API_KEY = configFileProvider.ApiKey;
            _messageHistory = new Dictionary<string, string>();
        }


        public void BroadcastMessage(SignalRMessage message)
        {
            string lastMessage;
            if (_messageHistory.TryGetValue(message.Name, out lastMessage))
            {
                if (message.Action == ModelAction.Updated && message.Body.ToJson() == lastMessage)
                {
                    return;
                }
            }

            _messageHistory[message.Name] = message.Body.ToJson();

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

        protected override Task OnConnected(IRequest request, string connectionId)
        {
            return SendVersion(connectionId);
        }

        protected override Task OnReconnected(IRequest request, string connectionId)
        {
            return SendVersion(connectionId);
        }

        private Task SendVersion(string connectionId)
        {
            return Context.Connection.Send(connectionId, new SignalRMessage
            {
                Name = "version",
                Body = new
                {
                    Version = BuildInfo.Version.ToString()
                }
            });
        }        
    }
}