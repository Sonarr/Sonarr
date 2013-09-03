using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Tracing;

namespace NzbDrone.Host.Owin
{
    public static class OwinServiceProviderFactory
    {
        public static ServiceProvider Create()
        {
           var  provider = (ServiceProvider)ServicesFactory.Create();
           provider.Add(typeof(ITraceOutputFactory), typeof(OwinTraceOutputFactory));

            return provider;
        }
    }
}