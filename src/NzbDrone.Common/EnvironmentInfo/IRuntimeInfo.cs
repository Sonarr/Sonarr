using System;

namespace NzbDrone.Common.EnvironmentInfo
{
    public interface IRuntimeInfo
    {
        DateTime StartTime { get; }
        bool IsUserInteractive { get; }
        bool IsAdmin { get; }
        bool IsWindowsService { get; }
        bool IsConsole { get; }
        bool IsRunning { get; set; }
        bool RestartPending { get; set; }
        string ExecutingApplication { get; }
        string RuntimeVersion { get; }
    }
}