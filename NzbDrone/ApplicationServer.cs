using System;
using System.ServiceProcess;
using NLog;
using NzbDrone.Common;
using NzbDrone.Owin;

namespace NzbDrone
{
    public interface INzbDroneServiceFactory
    {
        ServiceBase Build();
        void Start();
    }

    public class NzbDroneServiceFactory : ServiceBase, INzbDroneServiceFactory
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IEnvironmentProvider _environmentProvider;
        private readonly IHostController _hostController;
        private readonly IProcessProvider _processProvider;
        private readonly PriorityMonitor _priorityMonitor;
        private readonly IFirewallAdapter _firewallAdapter;
        private readonly IUrlAclAdapter _urlAclAdapter;
        private readonly Logger _logger;

        public NzbDroneServiceFactory(IConfigFileProvider configFileProvider, IHostController hostController,
                          IEnvironmentProvider environmentProvider,
                           IProcessProvider processProvider, PriorityMonitor priorityMonitor,
                           IFirewallAdapter firewallAdapter, IUrlAclAdapter urlAclAdapter, Logger logger)
        {
            _configFileProvider = configFileProvider;
            _hostController = hostController;
            _environmentProvider = environmentProvider;
            _processProvider = processProvider;
            _priorityMonitor = priorityMonitor;
            _firewallAdapter = firewallAdapter;
            _urlAclAdapter = urlAclAdapter;
            _logger = logger;
        }

        protected override void OnStart(string[] args)
        {
            Start();
        }

        public void Start()
        {
            if (_environmentProvider.IsAdmin)
            {
                _urlAclAdapter.RefreshRegistration();
                _firewallAdapter.MakeAccessible();

            }
            _hostController.StartServer();

            if (_environmentProvider.IsUserInteractive && _configFileProvider.LaunchBrowser)
            {
                try
                {
                    _logger.Info("Starting default browser. {0}", _hostController.AppUrl);
                    _processProvider.Start(_hostController.AppUrl);
                }
                catch (Exception e)
                {
                    _logger.ErrorException("Failed to open URL in default browser.", e);
                }
            }

            _priorityMonitor.Start();
        }

        protected override void OnStop()
        {
            _logger.Info("Attempting to stop application.");
            _hostController.StopServer();
            _logger.Info("Application has finished stop routine.");
        }

        public ServiceBase Build()
        {
            return this;
        }
    }

}