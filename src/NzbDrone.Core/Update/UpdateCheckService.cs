using System;
using System.Collections.Generic;
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
            var latestAvailable = _updatePackageProvider.GetLatestUpdate(_configFileProvider.Branch, BuildInfo.Version);

            if (OsInfo.IsNotWindows && !_configFileProvider.UpdateAutomatically)
            {
                return null;
            }

            if (latestAvailable == null)
            {
                _logger.ProgressDebug("No update available.");
            }
            else if (latestAvailable.Branch != _configFileProvider.Branch)
            {
                try
                {
                    _logger.Info("Branch [{0}] is being redirected to [{1}]]", _configFileProvider.Branch, latestAvailable.Branch);
                    var config = new Dictionary<string, object>();
                    config["Branch"] = latestAvailable.Branch;
                    _configFileProvider.SaveConfigDictionary(config);
                }
                catch (Exception e)
                {
                    _logger.ErrorException("Couldn't save the branch redirect.", e);
                }

            }

            return latestAvailable;
        }
    }
}