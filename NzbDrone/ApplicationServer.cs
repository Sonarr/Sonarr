using System;
using System.Net;
using System.ServiceProcess;
using NLog;
using Ninject;
using NzbDrone.Common;
using NzbDrone.Providers;


namespace NzbDrone
{
    public class ApplicationServer : ServiceBase
    {
        private static readonly Logger Logger = LogManager.GetLogger("Host.App");

        private readonly ConfigFileProvider _configFileProvider;
        private readonly DebuggerProvider _debuggerProvider;
        private readonly EnviromentProvider _enviromentProvider;
        private readonly IISProvider _iisProvider;
        private readonly ProcessProvider _processProvider;
        private readonly MonitoringProvider _monitoringProvider;
        private readonly SecurityProvider _securityProvider;
        private readonly WebClient _webClient;

        [Inject]
        public ApplicationServer(ConfigFileProvider configFileProvider, WebClient webClient, IISProvider iisProvider,
                           DebuggerProvider debuggerProvider, EnviromentProvider enviromentProvider,
                           ProcessProvider processProvider, MonitoringProvider monitoringProvider, SecurityProvider securityProvider)
        {
            _configFileProvider = configFileProvider;
            _webClient = webClient;
            _iisProvider = iisProvider;
            _debuggerProvider = debuggerProvider;
            _enviromentProvider = enviromentProvider;
            _processProvider = processProvider;
            _monitoringProvider = monitoringProvider;
            _securityProvider = securityProvider;
        }

        public ApplicationServer()
        {

        }

        protected override void OnStart(string[] args)
        {
            Start();
        }

        public virtual void Start()
        {
            _iisProvider.StopServer();
            _securityProvider.MakeAccessible();
            _iisProvider.StartServer();

            _debuggerProvider.Attach();

            if (_enviromentProvider.IsUserInteractive && _configFileProvider.LaunchBrowser)
            {
                try
                {
                    Logger.Info("Starting default browser. {0}", _iisProvider.AppUrl);
                    _processProvider.Start(_iisProvider.AppUrl);
                }
                catch (Exception e)
                {
                    Logger.ErrorException("Failed to open URL in default browser.", e);
                }
            }

            _monitoringProvider.Start();
        }

        protected override void OnStop()
        {
            Logger.Info("Attempting to stop application.");
            _iisProvider.StopServer();
            Logger.Info("Application has finished stop routine.");
        }
    }
}