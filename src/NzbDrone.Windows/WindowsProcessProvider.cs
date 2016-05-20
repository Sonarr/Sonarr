using NLog;
using NzbDrone.Common.Processes;

namespace NzbDrone.Windows
{
    public class WindowsProcessProvider : ProcessProviderBase
    {
        public WindowsProcessProvider(Logger logger)
            : base(logger)
        {
        }
    }
}
