using System;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using NLog;
using Ninject;
using NzbDrone.Common;
using NzbDrone.Providers;


namespace NzbDrone
{
    public class ApplicationServer : ServiceBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly ConfigFileProvider _configFileProvider;
        private readonly DebuggerProvider _debuggerProvider;
        private readonly EnvironmentProvider _environmentProvider;
        private readonly IISProvider _iisProvider;
        private readonly ProcessProvider _processProvider;
        private readonly MonitoringProvider _monitoringProvider;
        private readonly SecurityProvider _securityProvider;
        private readonly DiskProvider _diskProvider;

        [Inject]
        public ApplicationServer(ConfigFileProvider configFileProvider, IISProvider iisProvider,
                           DebuggerProvider debuggerProvider, EnvironmentProvider environmentProvider,
                           ProcessProvider processProvider, MonitoringProvider monitoringProvider,
                           SecurityProvider securityProvider, DiskProvider diskProvider)
        {
            _configFileProvider = configFileProvider;
            _iisProvider = iisProvider;
            _debuggerProvider = debuggerProvider;
            _environmentProvider = environmentProvider;
            _processProvider = processProvider;
            _monitoringProvider = monitoringProvider;
            _securityProvider = securityProvider;
            _diskProvider = diskProvider;
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

            if(_securityProvider.IsCurrentUserAdmin())
            {
                var tempFiles = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "Temporary ASP.NET Files");
                logger.Debug("Creating Temporary ASP.Net folder: {0}", tempFiles);
                _diskProvider.CreateDirectory(tempFiles);
            }

            _iisProvider.StartServer();

            _debuggerProvider.Attach();

            if (_environmentProvider.IsUserInteractive && _configFileProvider.LaunchBrowser)
            {
                try
                {
                    logger.Info("Starting default browser. {0}", _iisProvider.AppUrl);
                    _processProvider.Start(_iisProvider.AppUrl);
                }
                catch (Exception e)
                {
                    logger.ErrorException("Failed to open URL in default browser.", e);
                }
            }

            _monitoringProvider.Start();
        }

        protected override void OnStop()
        {
            logger.Info("Attempting to stop application.");
            _iisProvider.StopServer();
            logger.Info("Application has finished stop routine.");
        }
    }
}