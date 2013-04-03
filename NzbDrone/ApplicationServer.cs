using System;
using System.ServiceProcess;
using NLog;
using NzbDrone.Common;


namespace NzbDrone
{
    public class ApplicationServer : ServiceBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly ConfigFileProvider _configFileProvider;
        private readonly EnvironmentProvider _environmentProvider;
        private readonly IHostController _hostController;
        private readonly ProcessProvider _processProvider;
        private readonly PriorityMonitor _priorityMonitor;
        private readonly SecurityProvider _securityProvider;

        public ApplicationServer(ConfigFileProvider configFileProvider, IHostController hostController,
                          EnvironmentProvider environmentProvider,
                           ProcessProvider processProvider, PriorityMonitor priorityMonitor,
                           SecurityProvider securityProvider)
        {
            _configFileProvider = configFileProvider;
            _hostController = hostController;
            _environmentProvider = environmentProvider;
            _processProvider = processProvider;
            _priorityMonitor = priorityMonitor;
            _securityProvider = securityProvider;
        }

        protected override void OnStart(string[] args)
        {
            Start();
        }

        public virtual void Start()
        {
            _securityProvider.MakeAccessible();

            _hostController.StartServer();

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

            _priorityMonitor.Start();
        }

        protected override void OnStop()
        {
            logger.Info("Attempting to stop application.");
            _hostController.StopServer();
            logger.Info("Application has finished stop routine.");
        }
    }
}