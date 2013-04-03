using NLog;
using NzbDrone.Common;
using TinyIoC;
using NzbDrone.Core;
using NzbDrone.Api;

namespace NzbDrone
{
    public static class NzbDroneBootstrapper
    {
        private static readonly Logger logger = LogManager.GetLogger("NzbDroneBootstrapper");

        static NzbDroneBootstrapper()
        {
            InitializeApp();
        }


        private static void InitializeApp()
        {
            var environmentProvider = ContainerBuilder.Instance.Resolve<EnvironmentProvider>();

            ReportingService.RestProvider = ContainerBuilder.Instance.Resolve<RestProvider>();

            logger.Info("Start-up Path:'{0}'", environmentProvider.WorkingDirectory);
        }
    }
}
