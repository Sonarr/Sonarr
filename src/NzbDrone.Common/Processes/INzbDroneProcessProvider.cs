using System.Collections.Generic;
using NzbDrone.Common.Model;

namespace NzbDrone.Common.Processes
{
    public interface INzbDroneProcessProvider
    {
        List<ProcessInfo> GetNzbDroneProcesses();
    }
}
