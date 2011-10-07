using System;
using System.Net;
using System.Threading;
using NLog;
using NzbDrone.Providers;

namespace NzbDrone
{
    public class Application
    {
        private static readonly Logger Logger = LogManager.GetLogger("Application");

        private readonly ConfigProvider _configProvider;
        private readonly WebClient _webClient;
        private readonly IISProvider _iisProvider;
        private readonly ConsoleProvider _consoleProvider;
        private readonly DebuggerProvider _debuggerProvider;
        private readonly EnviromentProvider _enviromentProvider;
        private readonly ProcessProvider _processProvider;

        public Application(ConfigProvider configProvider, WebClient webClient, IISProvider iisProvider, ConsoleProvider consoleProvider,
            DebuggerProvider debuggerProvider, EnviromentProvider enviromentProvider, ProcessProvider processProvider)
        {
            _configProvider = configProvider;
            _webClient = webClient;
            _iisProvider = iisProvider;
            _consoleProvider = consoleProvider;
            _debuggerProvider = debuggerProvider;
            _enviromentProvider = enviromentProvider;
            _processProvider = processProvider;

            _configProvider.ConfigureNlog();
            _configProvider.CreateDefaultConfigFile();
            Logger.Info("Starting NZBDrone. Start-up Path:'{0}'", _configProvider.ApplicationRoot);
            Thread.CurrentThread.Name = "Host";

        }

        public void Start()
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

                _consoleProvider.WaitForClose();
                return;
            }

            try
            {
                _webClient.DownloadString(_iisProvider.AppUrl);
            }
            catch (Exception e)
            {
                Logger.ErrorException("Failed to load home page.", e);
            }
        }

        public void Stop()
        {

        }
    }
}

