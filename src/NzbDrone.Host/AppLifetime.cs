using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Host;

namespace NzbDrone.Host
{
    public class AppLifetime : IHostedService, IHandle<ApplicationShutdownRequested>
    {
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IStartupContext _startupContext;
        private readonly IBrowserService _browserService;
        private readonly IProcessProvider _processProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public AppLifetime(IHostApplicationLifetime appLifetime,
            IConfigFileProvider configFileProvider,
            IRuntimeInfo runtimeInfo,
            IStartupContext startupContext,
            IBrowserService browserService,
            IProcessProvider processProvider,
            IEventAggregator eventAggregator,
            Logger logger)
        {
            _appLifetime = appLifetime;
            _configFileProvider = configFileProvider;
            _runtimeInfo = runtimeInfo;
            _startupContext = startupContext;
            _browserService = browserService;
            _processProvider = processProvider;
            _eventAggregator = eventAggregator;
            _logger = logger;

            appLifetime.ApplicationStarted.Register(OnAppStarted);
            appLifetime.ApplicationStopped.Register(OnAppStopped);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void OnAppStarted()
        {
            _runtimeInfo.IsExiting = false;

            if (!_startupContext.Flags.Contains(StartupContext.NO_BROWSER)
                && _configFileProvider.LaunchBrowser)
            {
                _browserService.LaunchWebUI();
            }

            _eventAggregator.PublishEvent(new ApplicationStartedEvent());
        }

        private void OnAppStopped()
        {
            if (_runtimeInfo.RestartPending && !_runtimeInfo.IsWindowsService)
            {
                var restartArgs = GetRestartArgs();

                _logger.Info("Attempting restart with arguments: {0}", restartArgs);
                _processProvider.SpawnNewProcess(_runtimeInfo.ExecutingApplication, restartArgs);
            }
        }

        private void Shutdown()
        {
            _logger.Info("Attempting to stop application.");
            _logger.Info("Application has finished stop routine.");
            _runtimeInfo.IsExiting = true;
            _appLifetime.StopApplication();
        }

        private string GetRestartArgs()
        {
            var args = _startupContext.PreservedArguments;

            args += " /restart";

            if (!args.Contains("/nobrowser"))
            {
                args += " /nobrowser";
            }

            return args;
        }

        public void Handle(ApplicationShutdownRequested message)
        {
            if (!_runtimeInfo.IsWindowsService)
            {
                if (message.Restarting)
                {
                    _runtimeInfo.RestartPending = true;
                }

                LogManager.Configuration = null;
                Shutdown();
            }
        }
    }
}
