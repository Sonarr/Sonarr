using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Model;
using NzbDrone.Common.Processes;

namespace NzbDrone.Mono
{
    public class NzbDroneProcessProvider : INzbDroneProcessProvider
    {
        private readonly IProcessProvider _processProvider;

        public NzbDroneProcessProvider(IProcessProvider processProvider)
        {
            _processProvider = processProvider;
        }

        public List<ProcessInfo> FindNzbDroneProcesses()
        {
            var monoProcesses = _processProvider.FindProcessByName("mono");

            return monoProcesses.Where(c =>
            {
                var processArgs = _processProvider.StartAndCapture("ps", String.Format("-p {0} -o args=", c.Id));

                return processArgs.Standard.Any(p => p.Contains(ProcessProvider.NZB_DRONE_PROCESS_NAME + ".exe") ||
                                                     p.Contains(ProcessProvider.NZB_DRONE_CONSOLE_PROCESS_NAME + ".exe"));
            }).ToList();
        }
    }
}
