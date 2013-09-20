using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.Owin.Hosting;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Security;
using NzbDrone.Core.Configuration;
using NzbDrone.Host.AccessControl;
using NzbDrone.Host.Owin.MiddleWare;
using Owin;

namespace NzbDrone.Host.Owin
{
    public class OwinHostController : IHostController
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IEnumerable<IOwinMiddleWare> _owinMiddleWares;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IUrlAclAdapter _urlAclAdapter;
        private readonly IFirewallAdapter _firewallAdapter;
        private readonly Logger _logger;
        private IDisposable _host;

        public OwinHostController(IConfigFileProvider configFileProvider, IEnumerable<IOwinMiddleWare> owinMiddleWares,
            IRuntimeInfo runtimeInfo, IUrlAclAdapter urlAclAdapter, IFirewallAdapter firewallAdapter, Logger logger)
        {
            _configFileProvider = configFileProvider;
            _owinMiddleWares = owinMiddleWares;
            _runtimeInfo = runtimeInfo;
            _urlAclAdapter = urlAclAdapter;
            _firewallAdapter = firewallAdapter;
            _logger = logger;
        }

        public void StartServer()
        {
            IgnoreCertErrorPolicy.Register();
            var urlAcl = DetermineUrlAcl();

            var options = new StartOptions(urlAcl)
                {
                    ServerFactory = "Microsoft.Owin.Host.HttpListener"
                };

            _logger.Info("starting server on {0}", urlAcl);

            try
            {
                _host = WebApp.Start(OwinServiceProviderFactory.Create(), options, BuildApp);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException == null)
                {
                    throw;
                }

                if (ex.InnerException is HttpListenerException)
                {
                    throw new PortInUseException("Port {0} is already in use, please ensure NzbDrone is not already running.",
                             ex,
                             _configFileProvider.Port);
                }

                throw ex.InnerException;
            }
        }

        private void BuildApp(IAppBuilder appBuilder)
        {
            appBuilder.Properties["host.AppName"] = "NzbDrone";

            foreach (var middleWare in _owinMiddleWares.OrderBy(c => c.Order))
            {
                _logger.Debug("Attaching {0} to host", middleWare.GetType().Name);
                middleWare.Attach(appBuilder);
            }
        }

        public string AppUrl
        {
            get { return string.Format("http://localhost:{0}", _configFileProvider.Port); }
        }

        public void StopServer()
        {
            if (_host == null) return;

            _logger.Info("Attempting to stop Nancy host");
            _host.Dispose();
            _host = null;
            _logger.Info("Host has stopped");
        }

        private string DetermineUrlAcl()
        {
            if (OsInfo.IsWindows && _runtimeInfo.IsAdmin)
            {
                if (_runtimeInfo.IsAdmin)
                {
                    _urlAclAdapter.RefreshRegistration();
                    _firewallAdapter.MakeAccessible();
                }

                else
                {
                    if (!_urlAclAdapter.IsRegistered())
                    {
                        return _urlAclAdapter.LocalUrlAcl;
                    }
                }
            }

            return _urlAclAdapter.UrlAcl;
        }
    }
}