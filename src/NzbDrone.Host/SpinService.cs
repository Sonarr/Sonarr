using System.Threading;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Processes;

namespace NzbDrone.Host
{
    public interface IWaitForExit
    {
        void Spin();
    }

    public class SpinService : IWaitForExit
    {
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IProcessProvider _processProvider;
        private readonly Logger _logger;

        public SpinService(IRuntimeInfo runtimeInfo, IProcessProvider processProvider, Logger logger)
        {
            _runtimeInfo = runtimeInfo;
            _processProvider = processProvider;
            _logger = logger;
        }

        public void Spin()
        {
            while (_runtimeInfo.IsRunning)
            {
                Thread.Sleep(1000);
            }

            _logger.Debug("wait loop was terminated.");

            if (_runtimeInfo.RestartPending)
            {
                _logger.Info("attemptig restart.");
                _processProvider.SpawnNewProcess(_runtimeInfo.ExecutingApplication, "--restart --nobrowser");
            }
        }
    }
}
