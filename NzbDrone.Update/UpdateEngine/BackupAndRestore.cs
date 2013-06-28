using NLog;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Update.UpdateEngine
{
    public interface IBackupAndRestore
    {
        void BackUp(string source);
        void Restore(string target);
    }

    public class BackupAndRestore : IBackupAndRestore
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IAppDirectoryInfo _appDirectoryInfo;
        private readonly Logger _logger;

        public BackupAndRestore(IDiskProvider diskProvider, IAppDirectoryInfo appDirectoryInfo, Logger logger)
        {
            _diskProvider = diskProvider;
            _appDirectoryInfo = appDirectoryInfo;
            _logger = logger;
        }

        public void BackUp(string source)
        {
            _logger.Info("Creating backup of existing installation");
            _diskProvider.CopyDirectory(source, _appDirectoryInfo.GetUpdateBackUpFolder());
        }

        public void Restore(string target)
        {
            //TODO:this should ignore single file failures.
            _logger.Info("Attempting to rollback upgrade");
            _diskProvider.CopyDirectory(_appDirectoryInfo.GetUpdateBackUpFolder(), target);
        }
    }
}