using System;
using NLog;
using NzbDrone.Host.AccessControl;

namespace NzbDrone.Host.Owin
{
    public class OwinHostController : IHostController
    {
        private readonly IOwinAppFactory _owinAppFactory;
        private readonly IRemoteAccessAdapter _remoteAccessAdapter;
        private readonly IUrlAclAdapter _urlAclAdapter;
        private readonly Logger _logger;
        private IDisposable _owinApp;

        public OwinHostController(
                                  IOwinAppFactory owinAppFactory,
                                  IRemoteAccessAdapter remoteAccessAdapter,
                                  IUrlAclAdapter urlAclAdapter,
                                  Logger logger)
        {
            _owinAppFactory = owinAppFactory;
            _remoteAccessAdapter = remoteAccessAdapter;
            _urlAclAdapter = urlAclAdapter;
            _logger = logger;
        }

        public void StartServer()
        {
            _remoteAccessAdapter.MakeAccessible(true);

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
