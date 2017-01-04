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
        private readonly IStartupContext _startupContext;
        private readonly Logger _logger;

        public SpinService(IRuntimeInfo runtimeInfo, IProcessProvider processProvider, IStartupContext startupContext, Logger logger)
        {
            _runtimeInfo = runtimeInfo;
            _processProvider = processProvider;
            _startupContext = startupContext;
            _logger = logger;
        }

        public void Spin()
        {
            while (!_runtimeInfo.IsExiting)
            {
                Thread.Sleep(1000);
            }

            _logger.Debug("Wait loop was terminated.");

            if (_runtimeInfo.RestartPending)
            {
                var restartArgs = GetRestartArgs();

                _logger.Info("Attempting restart with arguments: {0}", restartArgs);
                _processProvider.SpawnNewProcess(_runtimeInfo.ExecutingApplication, restartArgs);
            }
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
    }
}
