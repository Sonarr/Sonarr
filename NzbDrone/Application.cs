using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Timers;
using NLog;
using NzbDrone.Providers;

namespace NzbDrone
{
    internal class Application
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

            AppDomain.CurrentDomain.UnhandledException += ((s, e) => AppDomainException(e));

            AppDomain.CurrentDomain.ProcessExit += ProgramExited;
            AppDomain.CurrentDomain.DomainUnload += ProgramExited;
        }

        internal void Start()
        {
            _iisProvider.StopServer();
            _iisProvider.StartServer();
            
            _debuggerProvider.Attach();

            var prioCheckTimer = new System.Timers.Timer(5000);
            prioCheckTimer.Elapsed += EnsurePriority;
            prioCheckTimer.Enabled = true;

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

        internal void Stop()
        {

        }


        private void AppDomainException(object excepion)
        {
            Console.WriteLine("EPIC FAIL: {0}", excepion);
            Logger.Fatal("EPIC FAIL: {0}", excepion);

#if RELEASE
            new Client
            {
                ApiKey = "43BBF60A-EB2A-4C1C-B09E-422ADF637265",
                ApplicationName = "NZBDrone",
                CurrentException = excepion as Exception
            }.Submit();
#endif
        }


        internal void EnsurePriority(object sender, ElapsedEventArgs e)
        {
            var currentProcessId = _processProvider.GetCurrentProcessId();
            if (_processProvider.GetProcessPriority(currentProcessId) != ProcessPriorityClass.Normal)
            {
                _processProvider.SetPriority(_processProvider.GetCurrentProcessId(), ProcessPriorityClass.Normal);
            }

            var iisProcessPriority = _processProvider.GetProcessPriority(_iisProvider.IISProcessId);
            if (iisProcessPriority != ProcessPriorityClass.Normal && iisProcessPriority != ProcessPriorityClass.AboveNormal)
            {
                _processProvider.SetPriority(_iisProvider.IISProcessId, ProcessPriorityClass.Normal);
            }
        }

        private void ProgramExited(object sender, EventArgs e)
        {
            _iisProvider.StopServer();
        }

    }



}

