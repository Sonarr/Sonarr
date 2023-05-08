using System.Collections.Generic;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Update.History;

namespace NzbDrone.Core.Update
{
    public interface IRecentUpdateProvider
    {
        List<UpdatePackage> GetRecentUpdatePackages();
    }

    public class RecentUpdateProvider : IRecentUpdateProvider
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IUpdatePackageProvider _updatePackageProvider;
        private readonly IUpdateHistoryService _updateHistoryService;

        public RecentUpdateProvider(IConfigFileProvider configFileProvider,
                                    IUpdatePackageProvider updatePackageProvider,
                                    IUpdateHistoryService updateHistoryService)
        {
            _configFileProvider = configFileProvider;
            _updatePackageProvider = updatePackageProvider;
            _updateHistoryService = updateHistoryService;
        }

        public List<UpdatePackage> GetRecentUpdatePackages()
        {
            var branch = _configFileProvider.Branch;
            var version = BuildInfo.Version;
            var prevVersion = _updateHistoryService.PreviouslyInstalled();
            return _updatePackageProvider.GetRecentUpdates(branch, version, prevVersion);
        }
    }
}
