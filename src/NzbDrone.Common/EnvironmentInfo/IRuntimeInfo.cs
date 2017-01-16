using System;

namespace NzbDrone.Common.EnvironmentInfo
{
    public interface IRuntimeInfo
    {
        bool IsUserInteractive { get; }
        bool IsAdmin { get; }
        bool IsWindowsService { get; }
        bool IsExiting { get; set; }
        bool RestartPending { get; set; }
        string ExecutingApplication { get; }
    }
}