using System.Linq;
using NzbDrone.Common;

namespace NzbDrone.Update.UpdateEngine
{
    public interface IDetectApplicationType
    {
        AppType GetAppType();
    }

    public class DetectApplicationType : IDetectApplicationType
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IProcessProvider _processProvider;

        public DetectApplicationType(IServiceProvider serviceProvider, IProcessProvider processProvider)
        {
            _serviceProvider = serviceProvider;
            _processProvider = processProvider;
        }

        public AppType GetAppType()
        {
            if (_serviceProvider.ServiceExist(ServiceProvider.NZBDRONE_SERVICE_NAME)
                && _serviceProvider.IsServiceRunning(ServiceProvider.NZBDRONE_SERVICE_NAME))
            {
                return AppType.Service;
            }

            if (_processProvider.GetProcessByName(ProcessProvider.NzbDroneConsoleProcessName).Any())
            {
                return AppType.Console;
            }

            return AppType.Normal;
        }
    }
}