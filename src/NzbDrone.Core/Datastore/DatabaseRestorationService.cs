using System;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Core.Datastore
{
    public interface IRestoreDatabase
    {
        void Restore();
    }

    public class DatabaseRestorationService : IRestoreDatabase
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IAppFolderInfo _appFolderInfo;
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(DatabaseRestorationService));

        public DatabaseRestorationService(IDiskProvider diskProvider, IAppFolderInfo appFolderInfo)
        {
            _diskProvider = diskProvider;
            _appFolderInfo = appFolderInfo;
        }

        public void Restore()
        {
            var dbRestorePath = _appFolderInfo.GetDatabaseRestore();

            if (!_diskProvider.FileExists(dbRestorePath))
            {
                return;
            }

            try
            {
                Logger.Info("Restoring Database");

                var dbPath = _appFolderInfo.GetDatabase();

                foreach (var suffix in new[] { "", "-shm", "-wal", "-journal" })
                {
                    var preRestorePath = dbPath + ".pre-restore" + suffix;

                    if (_diskProvider.FileExists(preRestorePath))
                    {
                        _diskProvider.DeleteFile(preRestorePath);
                    }

                    if (_diskProvider.FileExists(dbPath + suffix))
                    {
                        _diskProvider.MoveFile(dbPath + suffix, preRestorePath);
                    }
                }

                _diskProvider.MoveFile(dbRestorePath, dbPath);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to restore database");
                throw;
            }
        }
    }
}
