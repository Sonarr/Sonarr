using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Instrumentation;

namespace NzbDrone.Core.Update
{
    public interface ICheckUpdateService
    {
        UpdatePackage AvailableUpdate();
    }

    public class CheckUpdateService : ICheckUpdateService
    {
        private readonly IUpdatePackageProvider _updatePackageProvider;
        private readonly IConfigFileProvider _configFileProvider;

        private readonly Logger _logger;


        public CheckUpdateService(IUpdatePackageProvider updatePackageProvider, IConfigFileProvider configFileProvider, Logger logger)
        {
            _updatePackageProvider = updatePackageProvider;
            _configFileProvider = configFileProvider;
            _logger = logger;
        }

        public UpdatePackage AvailableUpdate()
        {
            if (OsInfo.IsMono) return null;

            var latestAvailable = _updatePackageProvider.GetLatestUpdate(_configFileProvider.Branch, BuildInfo.Version);

            if (latestAvailable == null)
            {
                _logger.ProgressDebug("No update available.");
            }

            return latestAvailable;
        }
    }
}