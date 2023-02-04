using System;

namespace NzbDrone.Common.EnvironmentInfo
{
    public interface IRuntimeInfo
    {
        DateTime StartTime { get; }
        bool IsUserInteractive { get; }
        bool IsAdmin { get; }
        bool IsWindowsService { get; }
        bool IsWindowsTray { get; }
        bool IsStarting { get; set; }
        bool IsExiting { get; set; }
        bool IsTray { get; }
        RuntimeMode Mode { get; }
        bool RestartPending { get; set; }
        string ExecutingApplication { get; }
    }
}
