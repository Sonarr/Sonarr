using System;

namespace NzbDrone.Common.EnvironmentInfo
{
    public interface IRuntimeInfo
    {
        Boolean IsUserInteractive { get; }
        Boolean IsAdmin { get; }
        Boolean IsWindowsService { get; }
        Boolean IsConsole { get; }
        Boolean IsRunning { get; set; }
        Boolean RestartPending { get; set; }
        String ExecutingApplication { get; }
        String RuntimeVersion { get; }
    }
}