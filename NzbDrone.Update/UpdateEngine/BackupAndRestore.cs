using NLog;
using NzbDrone.Common;

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
        private readonly IEnvironmentProvider _environmentProvider;
        private readonly Logger _logger;

        public BackupAndRestore(IDiskProvider diskProvider, IEnvironmentProvider environmentProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _environmentProvider = environmentProvider;
            _logger = logger;
        }

        public void BackUp(string source)
        {
            _logger.Info("Creating backup of existing installation");
            _diskProvider.CopyDirectory(source, _environmentProvider.GetUpdateBackUpFolder());
        }

        public void Restore(string target)
        {
            //TODO:this should ignore single file failures.
            _logger.Info("Attempting to rollback upgrade");
            _diskProvider.CopyDirectory(_environmentProvider.GetUpdateBackUpFolder(), target);
        }
    }
}