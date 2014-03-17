using System.Threading;
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

        public SpinService(IRuntimeInfo runtimeInfo, IProcessProvider processProvider)
        {
            _runtimeInfo = runtimeInfo;
            _processProvider = processProvider;
        }

        public void Spin()
        {
            while (_runtimeInfo.IsRunning)
            {
                Thread.Sleep(1000);
            }

            if (_runtimeInfo.RestartPending)
            {
                _processProvider.SpawnNewProcess(_runtimeInfo.ExecutingApplication, "--restart --nobrowser");
            }
        }
    }
}
