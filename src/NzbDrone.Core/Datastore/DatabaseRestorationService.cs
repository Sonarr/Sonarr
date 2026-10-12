using System;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Datastore
{
    public interface IRestoreDatabase
    {
        void Restore();
    }

    public class DatabaseRestorationService : IRestoreDatabase, IHandleAsync<ApplicationStartedEvent>
    {
        private static readonly string[] DatabaseFileSuffixes = { "", "-shm", "-wal", "-journal" };

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

                foreach (var suffix in DatabaseFileSuffixes)
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

        public void HandleAsync(ApplicationStartedEvent message)
        {
            var dbPath = _appFolderInfo.GetDatabase();

            foreach (var suffix in DatabaseFileSuffixes)
            {
                var preRestorePath = dbPath + ".pre-restore" + suffix;

                if (_diskProvider.FileExists(preRestorePath))
                {
                    _diskProvider.DeleteFile(preRestorePath);
                }
            }
        }
    }
}
