using System.IO;
using NLog;
using NzbDrone.Common;

namespace NzbDrone.Update.UpdateEngine
{
    public interface IStartNzbDrone
    {
        void Start(AppType appType, string installationFolder);
    }

    public class StartNzbDrone : IStartNzbDrone
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IProcessProvider _processProvider;
        private readonly Logger _logger;

        public StartNzbDrone(IServiceProvider serviceProvider, IProcessProvider processProvider, Logger logger)
        {
            _serviceProvider = serviceProvider;
            _processProvider = processProvider;
            _logger = logger;
        }

        public void Start(AppType appType, string installationFolder)
        {
            _logger.Info("Starting NzbDrone");
            if (appType == AppType.Service)
            {
                _logger.Info("Starting NzbDrone service");
                _serviceProvider.Start(ServiceProvider.NZBDRONE_SERVICE_NAME);
            }
            else if (appType == AppType.Console)
            {
                _logger.Info("Starting NzbDrone with Console");
                _processProvider.Start(Path.Combine(installationFolder, "NzbDrone.Console.exe"));
            }
            else
            {
                _logger.Info("Starting NzbDrone without Console");
                _processProvider.Start(Path.Combine(installationFolder, "NzbDrone.exe"));
            }
        }
    }
}