using System;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using NLog;
using NzbDrone.Common;
using NzbDrone.Providers;


namespace NzbDrone
{
    public class ApplicationServer : ServiceBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly ConfigFileProvider _configFileProvider;
        private readonly EnvironmentProvider _environmentProvider;
        private readonly HostController _hostController;
        private readonly ProcessProvider _processProvider;
        private readonly MonitoringProvider _monitoringProvider;
        private readonly SecurityProvider _securityProvider;
        private readonly DiskProvider _diskProvider;

        public ApplicationServer(ConfigFileProvider configFileProvider, HostController hostController,
                          EnvironmentProvider environmentProvider,
                           ProcessProvider processProvider, MonitoringProvider monitoringProvider,
                           SecurityProvider securityProvider, DiskProvider diskProvider)
        {
            _configFileProvider = configFileProvider;
            _hostController = hostController;
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
            _hostController.StopServer();
            _securityProvider.MakeAccessible();

            if(_securityProvider.IsCurrentUserAdmin())
            {
                var tempFiles = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "Temporary ASP.NET Files");
                logger.Debug("Creating Temporary ASP.Net folder: {0}", tempFiles);
                _diskProvider.CreateDirectory(tempFiles);
            }

            _hostController.StartServer();
            //Todo: verify that IIS is actually started


            if (_environmentProvider.IsUserInteractive && _configFileProvider.LaunchBrowser)
            {
                try
                {
                    logger.Info("Starting default browser. {0}", _hostController.AppUrl);
                    _processProvider.Start(_hostController.AppUrl);
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
            _hostController.StopServer();
            logger.Info("Application has finished stop routine.");
        }
    }
}