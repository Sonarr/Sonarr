using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
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
            _logger.Trace("SignalR client connected. ID:{0}", connectionId);
            return base.OnConnected(request, connectionId);
        }

        public void HandleAsync(ModelEvent<T> message)
        {
            var context = ((ConnectionManager)GlobalHost.ConnectionManager).GetConnection(GetType());
            context.Connection.Broadcast(message);
        }
    }
}