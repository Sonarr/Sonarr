using System;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Configuration;

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


        public CheckUpdateService(IUpdatePackageProvider updatePackageProvider,
                                  IConfigFileProvider configFileProvider,
                                  Logger logger)
        {
            _updatePackageProvider = updatePackageProvider;
            _configFileProvider = configFileProvider;
            _logger = logger;
        }

        public UpdatePackage AvailableUpdate()
        {
            if (OsInfo.IsMono && !_configFileProvider.UpdateAutomatically)
            {
                return null;
            }

            var latestAvailable = _updatePackageProvider.GetLatestUpdate(_configFileProvider.Branch, BuildInfo.Version);

            if (latestAvailable == null)
            {
                _logger.ProgressDebug("No update available.");
            }
            else if (latestAvailable.Branch != _configFileProvider.Branch)
            {
                try
                {
                    _logger.Warn("[{0}] isn't a valid branch. Switching back to [master]", _configFileProvider.Branch);
                    var config = _configFileProvider.GetConfigDictionary();
                    config["Branch"] = _configFileProvider.Branch;
                    _configFileProvider.SaveConfigDictionary(config);
                }
                catch (Exception e)
                {
                    _logger.ErrorException("couldn't revert back to master.", e);
                }

            }

            return latestAvailable;
        }
    }
}