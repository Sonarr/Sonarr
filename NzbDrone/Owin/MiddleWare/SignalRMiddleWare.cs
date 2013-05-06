using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using NzbDrone.Api.SignalR;
using Owin;
using TinyIoC;

namespace NzbDrone.Owin.MiddleWare
{
    public class SignalRMiddleWare : IOwinMiddleWare
    {
        private readonly IEnumerable<NzbDronePersistentConnection> _persistentConnections;

        public int Order { get { return 0; } }

        public SignalRMiddleWare(IEnumerable<NzbDronePersistentConnection> persistentConnections, TinyIoCContainer container)
        {
            _persistentConnections = persistentConnections;

            SignalrDependencyResolver.Register(container);
        }

        public void Attach(IAppBuilder appBuilder)
        {
            foreach (var nzbDronePersistentConnection in _persistentConnections)
            {
                var url = string.Format("signalr/{0}", nzbDronePersistentConnection.Resource.Trim('/'));
                appBuilder.MapConnection(url, nzbDronePersistentConnection.GetType(), new ConnectionConfiguration { EnableCrossDomain = true });
            }

        }
    }
}