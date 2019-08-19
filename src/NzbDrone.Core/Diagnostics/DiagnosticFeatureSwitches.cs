using System.IO;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Core.Diagnostics
{
    public interface IDiagnosticFeatureSwitches
    {
        bool ScriptConsoleEnabled { get; }
    }

    public class DiagnosticFeatureSwitches : IDiagnosticFeatureSwitches
    {
        private IDiskProvider _diskProvider;
        private IAppFolderInfo _appFolderInfo;

        public DiagnosticFeatureSwitches(IDiskProvider diskProvider, IAppFolderInfo appFolderInfo)
        {
            _diskProvider = diskProvider;
            _appFolderInfo = appFolderInfo;
        }

        public bool ScriptConsoleEnabled
        {
            get
            {
                // Only allow this if the 'debugscripts' config folder exists.
                // Scripting is a significant security risk with only an api key for protection.
                if (!_diskProvider.FolderExists(Path.Combine(_appFolderInfo.AppDataFolder, "debugscripts")))
                {
                    return false;
                }

                return true;
            }
        }
    }
}
