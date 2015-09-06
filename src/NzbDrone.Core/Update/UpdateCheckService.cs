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
        UpdatePackage LatestUpdate();

    }

    public class CheckUpdateService : ICheckUpdateService
    {
        private readonly IUpdatePackageProvider _updatePackageProvider;
        private readonly IConfigFileProvider _configFileProvider;

        private readonly Logger _logger;
        private UpdatePackage _latestUpdate;

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
            _latestUpdate = _updatePackageProvider.GetLatestUpdate(_configFileProvider.Branch, BuildInfo.Version);
            return _latestUpdate;
        }

        public UpdatePackage LatestUpdate()
        {
            return _latestUpdate;
        }
    }
}