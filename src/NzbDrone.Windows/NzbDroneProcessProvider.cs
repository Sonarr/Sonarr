using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Model;
using NzbDrone.Common.Processes;

namespace NzbDrone.Windows
{
    public class NzbDroneProcessProvider : INzbDroneProcessProvider
    {
        private readonly IProcessProvider _processProvider;

        public NzbDroneProcessProvider(IProcessProvider processProvider)
        {
            _processProvider = processProvider;
        }

        public List<ProcessInfo> GetNzbDroneProcesses()
        {
            var consoleProcesses = _processProvider.FindProcessByName(ProcessProvider.NZB_DRONE_CONSOLE_PROCESS_NAME);
            var winformProcesses = _processProvider.FindProcessByName(ProcessProvider.NZB_DRONE_PROCESS_NAME);

            return consoleProcesses.Concat(winformProcesses).ToList();
        }
    }
}
