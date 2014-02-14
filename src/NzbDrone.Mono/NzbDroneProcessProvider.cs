using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Model;
using NzbDrone.Common.Processes;

namespace NzbDrone.Mono
{
    public class NzbDroneProcessProvider : INzbDroneProcessProvider
    {
        private readonly IProcessProvider _processProvider;
        private readonly Logger _logger;

        public NzbDroneProcessProvider(IProcessProvider processProvider, Logger logger)
        {
            _processProvider = processProvider;
            _logger = logger;
        }

        public List<ProcessInfo> FindNzbDroneProcesses()
        {
            var monoProcesses = _processProvider.FindProcessByName("mono");

            return monoProcesses.Where(c =>
            {
                try
                {
                    var processArgs = _processProvider.StartAndCapture("ps", String.Format("-p {0} -o args=", c.Id));

                    return processArgs.Standard.Any(p => p.Contains(ProcessProvider.NZB_DRONE_PROCESS_NAME + ".exe") ||
                                                         p.Contains(ProcessProvider.NZB_DRONE_CONSOLE_PROCESS_NAME + ".exe"));
                }
                catch (InvalidOperationException ex)
                {
                    _logger.WarnException("Error getting process arguments", ex);
                    return false;
                }
                
            }).ToList();
        }
    }
}
