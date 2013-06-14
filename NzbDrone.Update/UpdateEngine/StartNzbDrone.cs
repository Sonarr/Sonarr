using System;
using System.IO;
using NLog;
using NzbDrone.Common;
using IServiceProvider = NzbDrone.Common.IServiceProvider;

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
                try
                {
                    StartService();

                }
                catch (InvalidOperationException e)
                {
                    _logger.Warn("Couldn't start NzbDrone Service (Most likely due to permission issues). falling back to console.", e);
                    StartConsole(installationFolder);
                }
            }
            else if (appType == AppType.Console)
            {
                StartConsole(installationFolder);
            }
            else
            {
                StartWinform(installationFolder);
            }
        }

        private void StartService()
        {
            _logger.Info("Starting NzbDrone service");
            _serviceProvider.Start(ServiceProvider.NZBDRONE_SERVICE_NAME);
        }

        private void StartWinform(string installationFolder)
        {
            _logger.Info("Starting NzbDrone without Console");
            _processProvider.Start(Path.Combine(installationFolder, "NzbDrone.exe"));
        }

        private void StartConsole(string installationFolder)
        {
            _logger.Info("Starting NzbDrone with Console");
            _processProvider.Start(Path.Combine(installationFolder, "NzbDrone.Console.exe"));
        }
    }
}