using Microsoft.AspNet.SignalR;

namespace NzbDrone.Api.SignalR
{
    public abstract class NzbDronePersistentConnection : PersistentConnection
    {
        public abstract string Resource { get; }
    }
}