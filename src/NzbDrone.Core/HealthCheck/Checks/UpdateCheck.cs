using System;
using System.IO;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Update;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class UpdateCheck : IProvideHealthCheck
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly ICheckUpdateService _checkUpdateService;

        public UpdateCheck(IDiskProvider diskProvider, IAppFolderInfo appFolderInfo, ICheckUpdateService checkUpdateService)
        {
            _diskProvider = diskProvider;
            _appFolderInfo = appFolderInfo;
            _checkUpdateService = checkUpdateService;
        }


        public HealthCheck Check()
        {
            if (OsInfo.IsWindows)
            {
                try
                {
                    var testPath = Path.Combine(_appFolderInfo.StartUpFolder, "drone_test.txt");
                    _diskProvider.WriteAllText(testPath, DateTime.Now.ToString());
                    _diskProvider.DeleteFile(testPath);
                }
                catch (Exception)
                {
                    return new HealthCheck(HealthCheckResultType.Error,
                        "Unable to update, running from write-protected folder");
                }
            }

            if (BuildInfo.BuildDateTime < DateTime.UtcNow.AddDays(-14))
            {
                if (_checkUpdateService.AvailableUpdate() != null)
                {
                    return new HealthCheck(HealthCheckResultType.Warning, "New update is available");
                }
            }

            return null;
        }
    }
}
