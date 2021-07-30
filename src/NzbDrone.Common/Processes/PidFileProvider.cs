using System;
using System.IO;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Common.Processes
{
    public interface IProvidePidFile
    {
        void Write();
    }

    public class PidFileProvider : IProvidePidFile
    {
        private readonly IAppFolderInfo _appFolderInfo;
        private readonly Logger _logger;

        public PidFileProvider(IAppFolderInfo appFolderInfo, Logger logger)
        {
            _appFolderInfo = appFolderInfo;
            _logger = logger;
        }

        public void Write()
        {
            if (OsInfo.IsWindows)
            {
                return;
            }

            var filename = Path.Combine(_appFolderInfo.AppDataFolder, "sonarr.pid");
            try
            {
                File.WriteAllText(filename, ProcessProvider.GetCurrentProcessId().ToString());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to write PID file: " + filename);
                throw new SonarrStartupException(ex, "Unable to write PID file {0}", filename);
            }
        }
    }
}
