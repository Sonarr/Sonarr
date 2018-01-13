using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Hosting.Engine;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Tracing;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Host.Owin.MiddleWare;
using Owin;

namespace NzbDrone.Host.Owin
{
    public interface IOwinAppFactory
    {
        IDisposable CreateApp(List<string> urls);
    }

    public class OwinAppFactory : IOwinAppFactory
    {
        private readonly IEnumerable<IOwinMiddleWare> _owinMiddleWares;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly Logger _logger;

        public OwinAppFactory(IEnumerable<IOwinMiddleWare> owinMiddleWares, IConfigFileProvider configFileProvider, Logger logger)
        {
            _owinMiddleWares = owinMiddleWares;
            _configFileProvider = configFileProvider;
            _logger = logger;
        }

        public IDisposable CreateApp(List<string> urls)
        {
            var services = CreateServiceFactory();
            var engine = services.GetService<IHostingEngine>();

            var options = new StartOptions()
            {
                ServerFactory = "Microsoft.Owin.Host.HttpListener"
            };

            urls.ForEach(options.Urls.Add);

            var context = new StartContext(options) { Startup = BuildApp };


            try
            {
                return engine.Start(context);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException == null)
                {
                    throw;
                }

                if (ex.InnerException is HttpListenerException)
                {
                    throw new PortInUseException("Port {0} is already in use, please ensure NzbDrone is not already running.", ex, _configFileProvider.Port);
                }

                throw ex.InnerException;
            }
        }


        private void BuildApp(IAppBuilder appBuilder)
        {
            appBuilder.Properties["host.AppName"] = "Sonarr";

            foreach (var middleWare in _owinMiddleWares.OrderBy(c => c.Order))
            {
                _logger.Debug("Attaching {0} to host", middleWare.GetType().Name);
                middleWare.Attach(appBuilder);
            }
        }


        private IServiceProvider CreateServiceFactory()
        {
            var provider = (ServiceProvider)ServicesFactory.Create();
            provider.Add(typeof(ITraceOutputFactory), typeof(OwinTraceOutputFactory));

            return provider;
        }
    }
}
