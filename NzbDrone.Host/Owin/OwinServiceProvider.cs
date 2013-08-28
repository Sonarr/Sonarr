using System;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Tracing;

namespace NzbDrone.Host.Owin
{
    public class OwinServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider _defaultProvider;

        public OwinServiceProvider()
        {
          _defaultProvider =   ServicesFactory.Create();
        }
        public object GetService(Type serviceType)
        {
            if (serviceType == typeof (ITraceOutputFactory))
            {
                return new OwinTraceOutputFactory();
            }

            return _defaultProvider.GetService(serviceType);
        }
    }
}