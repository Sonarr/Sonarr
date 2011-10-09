using System;
using System.Net;
using System.Threading;
using NLog;
using Ninject;
using NzbDrone.Providers;

namespace NzbDrone
{
    public class Application
    {
        private static readonly Logger Logger = LogManager.GetLogger("Host.App");

        private readonly ConfigProvider _configProvider;
        private readonly DebuggerProvider _debuggerProvider;
        private readonly EnviromentProvider _enviromentProvider;
        private readonly IISProvider _iisProvider;
        private readonly ProcessProvider _processProvider;
        private readonly WebClient _webClient;

        [Inject]
        public Application(ConfigProvider configProvider, WebClient webClient, IISProvider iisProvider,
                           DebuggerProvider debuggerProvider, EnviromentProvider enviromentProvider,
                           ProcessProvider processProvider)
        {
            _configProvider = configProvider;
            _webClient = webClient;
            _iisProvider = iisProvider;
            _debuggerProvider = debuggerProvider;
            _enviromentProvider = enviromentProvider;
            _processProvider = processProvider;

            _configProvider.ConfigureNlog();
            _configProvider.CreateDefaultConfigFile();
            Logger.Info("Starting NZBDrone. Start-up Path:'{0}'", _enviromentProvider.ApplicationPath);
            Thread.CurrentThread.Name = "Host";
        }

        public Application()
        {
        }

        public virtual void Start()
        {
            _iisProvider.StopServer();
            _iisProvider.StartServer();

            _debuggerProvider.Attach();

            if (_enviromentProvider.IsUserInteractive && _configProvider.LaunchBrowser)
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
            else
            {
                try
                {
                    _webClient.DownloadString(_iisProvider.AppUrl);
                }
                catch (Exception e)
                {
                    Logger.ErrorException("Failed to load home page.", e);
                }
            }
        }

        public virtual void Stop()
        {
            Logger.Info("Attempting to stop application.");
            _iisProvider.StopServer();
            Logger.Info("Application has finished stop routine.");
        }
    }
}