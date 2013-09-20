using System;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Processes;
using IServiceProvider = NzbDrone.Common.IServiceProvider;

namespace NzbDrone.Update.UpdateEngine
{
    public interface ITerminateNzbDrone
    {
        void Terminate();
    }

    public class TerminateNzbDrone : ITerminateNzbDrone
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IProcessProvider _processProvider;
        private readonly Logger _logger;

        public TerminateNzbDrone(IServiceProvider serviceProvider, IProcessProvider processProvider, Logger logger)
        {
            _serviceProvider = serviceProvider;
            _processProvider = processProvider;
            _logger = logger;
        }

        public void Terminate()
        {
            _logger.Info("Stopping all running services");

            if (_serviceProvider.ServiceExist(ServiceProvider.NZBDRONE_SERVICE_NAME)
                && _serviceProvider.IsServiceRunning(ServiceProvider.NZBDRONE_SERVICE_NAME))
            {
                try
                {
                    _logger.Info("NzbDrone Service is installed and running");
                    _serviceProvider.Stop(ServiceProvider.NZBDRONE_SERVICE_NAME);

                }
                catch (Exception e)
                {
                    _logger.ErrorException("couldn't stop service", e);
                }
            }

            _logger.Info("Killing all running processes");

            _processProvider.KillAll(ProcessProvider.NZB_DRONE_CONSOLE_PROCESS_NAME);
            _processProvider.KillAll(ProcessProvider.NZB_DRONE_PROCESS_NAME);
        }
    }
}