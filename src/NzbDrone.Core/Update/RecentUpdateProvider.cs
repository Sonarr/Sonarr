using System.Collections.Generic;
using NzbDrone.Core.Configuration;

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

        public RecentUpdateProvider(IConfigFileProvider configFileProvider,
                                   IUpdatePackageProvider updatePackageProvider)
        {
            _configFileProvider = configFileProvider;
            _updatePackageProvider = updatePackageProvider;
        }

        public List<UpdatePackage> GetRecentUpdatePackages()
        {
            var branch = _configFileProvider.Branch;
            return _updatePackageProvider.GetRecentUpdates(branch);
        }
    }
}
