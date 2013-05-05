using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using NLog;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Events;

namespace NzbDrone.Api.SignalR
{
    public abstract class BasicResourceConnection<T> :
        NzbDronePersistentConnection,
        IHandleAsync<ModelEvent<T>>
        where T : ModelBase
    {
        private readonly Logger _logger;

        public BasicResourceConnection()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        protected override Task OnConnected(IRequest request, string connectionId)
        {
            _logger.Debug("SignalR client connected. ID:{0}", connectionId);
            return base.OnConnected(request, connectionId);
        }

        public override Task ProcessRequest(Microsoft.AspNet.SignalR.Hosting.HostContext context)
        {
            _logger.Debug("Request: {0}", context);
            return base.ProcessRequest(context);
        }

        public void HandleAsync(ModelEvent<T> message)
        {
            Connection.Broadcast(message);
        }
    }
}