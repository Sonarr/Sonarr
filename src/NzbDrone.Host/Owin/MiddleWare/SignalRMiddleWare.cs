using System;
using Microsoft.AspNet.SignalR;
using NzbDrone.Common.Composition;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.SignalR;
using Owin;

namespace NzbDrone.Host.Owin.MiddleWare
{
    public class SignalRMiddleWare : IOwinMiddleWare
    {
        public int Order => 1;

        public SignalRMiddleWare(IContainer container)
        {
            SignalRDependencyResolver.Register(container);
            SignalRJsonSerializer.Register();

            // Note there are some important timeouts involved here:
            // nginx has a default 60 sec proxy_read_timeout, this means the connection will be terminated if the server doesn't send anything within that time.
            // Previously we lowered the ConnectionTimeout from 110s to 55s to remedy that, however all we should've done is set an appropriate KeepAlive.
            // By default KeepAlive is 1/3rd of the DisconnectTimeout, which we set incredibly high 5 years ago, resulting in KeepAlive being 1 minute.
            // So when adjusting these values in the future, please keep that all in mind.
            GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromSeconds(110);
            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(180);
            GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(30);
        }

        public void Attach(IAppBuilder appBuilder)
        {
            appBuilder.MapSignalR("/signalr", typeof(NzbDronePersistentConnection), new ConnectionConfiguration());
        }
    }
}
