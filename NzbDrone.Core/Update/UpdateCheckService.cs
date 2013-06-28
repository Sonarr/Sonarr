using NLog;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Core.Update
{
    public interface ICheckUpdateService
    {
        UpdatePackage AvailableUpdate();
    }


    public class CheckUpdateService : ICheckUpdateService
    {
        private readonly IUpdatePackageProvider _updatePackageProvider;

        private readonly Logger _logger;


        public CheckUpdateService(IUpdatePackageProvider updatePackageProvider, Logger logger)
        {
            _updatePackageProvider = updatePackageProvider;
            _logger = logger;
        }

        public UpdatePackage AvailableUpdate()
        {
            var latestAvailable = _updatePackageProvider.GetLatestUpdate();

            if (latestAvailable == null || latestAvailable.Version <= BuildInfo.Version)
            {
                _logger.Debug("No update available.");
                return null;
            }

            return latestAvailable;
        }
    }
}