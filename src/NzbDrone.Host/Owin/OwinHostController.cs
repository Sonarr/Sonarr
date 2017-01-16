using System;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Host.AccessControl;

namespace NzbDrone.Host.Owin
{
    public class OwinHostController : IHostController
    {
        private readonly IOwinAppFactory _owinAppFactory;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IUrlAclAdapter _urlAclAdapter;
        private readonly IFirewallAdapter _firewallAdapter;
        private readonly ISslAdapter _sslAdapter;
        private readonly Logger _logger;
        private IDisposable _owinApp;

        public OwinHostController(
                                  IOwinAppFactory owinAppFactory,
                                  IRuntimeInfo runtimeInfo,
                                  IUrlAclAdapter urlAclAdapter,
                                  IFirewallAdapter firewallAdapter,
                                  ISslAdapter sslAdapter,
                                  Logger logger)
        {
            _owinAppFactory = owinAppFactory;
            _runtimeInfo = runtimeInfo;
            _urlAclAdapter = urlAclAdapter;
            _firewallAdapter = firewallAdapter;
            _sslAdapter = sslAdapter;
            _logger = logger;
        }

        public void StartServer()
        {
            if (OsInfo.IsWindows)
            {
                if (_runtimeInfo.IsAdmin)
                {
                    _firewallAdapter.MakeAccessible();
                    _sslAdapter.Register();
                }
            }

            _urlAclAdapter.ConfigureUrls();

            _logger.Info("Listening on the following URLs:");
            foreach (var url in _urlAclAdapter.Urls)
            {
                _logger.Info("  {0}", url);
            }

            _owinApp = _owinAppFactory.CreateApp(_urlAclAdapter.Urls);
        }


        public void StopServer()
        {
            if (_owinApp == null) return;

            _logger.Info("Attempting to stop OWIN host");
            _owinApp.Dispose();
            _owinApp = null;
            _logger.Info("Host has stopped");
        }



    }
}