using System;
using System.IO;
using NLog;

namespace NzbDrone.Update.UpdateEngine
{
    public interface IDetectExistingVersion
    {
        string GetExistingVersion(string targetFolder);
    }

    public class DetectExistingVersion : IDetectExistingVersion
    {
        private readonly Logger _logger;

        public DetectExistingVersion(Logger logger)
        {
            _logger = logger;
        }

        public string GetExistingVersion(string targetFolder)
        {
            try
            {
                var targetExecutable = Path.Combine(targetFolder, "Sonarr.exe");

                if (File.Exists(targetExecutable))
                {
                    var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(targetExecutable);

                    return versionInfo.FileVersion;
                }
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to get existing version from {0}", targetFolder);
            }

            return "(unknown)";
        }
    }
}
