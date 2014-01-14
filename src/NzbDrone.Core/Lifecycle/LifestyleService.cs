using System.IO;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Lifecycle.Commands;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using IServiceProvider = NzbDrone.Common.IServiceProvider;

namespace NzbDrone.Core.Lifecycle
{
    public class LifestyleService: IExecute<ShutdownCommand>, IExecute<RestartCommand>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly IServiceProvider _serviceProvider;
        private readonly IProcessProvider _processProvider;


        public LifestyleService(IEventAggregator eventAggregator,
                                IRuntimeInfo runtimeInfo,
                                IAppFolderInfo appFolderInfo,
                                IServiceProvider serviceProvider,
                                IProcessProvider processProvider)
        {
            _eventAggregator = eventAggregator;
            _runtimeInfo = runtimeInfo;
            _appFolderInfo = appFolderInfo;
            _serviceProvider = serviceProvider;
            _processProvider = processProvider;
        }

        public void Execute(ShutdownCommand message)
        {
            _eventAggregator.PublishEvent(new ApplicationShutdownRequested());
            
            if (_runtimeInfo.IsWindowsService)
            {
                _serviceProvider.Stop(ServiceProvider.NZBDRONE_SERVICE_NAME);
            }
        }

        public void Execute(RestartCommand message)
        {
            _eventAggregator.PublishEvent(new ApplicationShutdownRequested(true));

            if (_runtimeInfo.IsWindowsService)
            {
                _serviceProvider.Restart(ServiceProvider.NZBDRONE_SERVICE_NAME);
            }

            else
            {
                //TODO: move this to environment specific projects
                if (OsInfo.IsWindows)
                {
                    if (_runtimeInfo.IsConsole)
                    {
                        //Run console with switch
                        var path = Path.Combine(_appFolderInfo.StartUpFolder,
                            ProcessProvider.NZB_DRONE_CONSOLE_PROCESS_NAME + ".exe");

                        _processProvider.SpawnNewProcess(path, "--terminateexisting --nobrowser");
                    }

                    else
                    {
                        var path = Path.Combine(_appFolderInfo.StartUpFolder,
                            ProcessProvider.NZB_DRONE_PROCESS_NAME + ".exe");

                        _processProvider.Start(path, "--terminateexisting --nobrowser");
                    }
                }
            }
        }
    }
}
