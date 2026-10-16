using System;
using System.Collections.Generic;
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
        private const string PreRestoreSuffix = ".pre-restore";
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

            var dbPath = _appFolderInfo.GetDatabase();
            var movedSuffixes = new List<string>();

            try
            {
                Logger.Info("Restoring Database");

                foreach (var suffix in DatabaseFileSuffixes)
                {
                    _diskProvider.DeleteFile(dbPath + PreRestoreSuffix + suffix);

                    if (_diskProvider.FileExists(dbPath + suffix))
                    {
                        _diskProvider.MoveFile(dbPath + suffix, dbPath + PreRestoreSuffix + suffix);
                        movedSuffixes.Add(suffix);
                    }
                }

                _diskProvider.MoveFile(dbRestorePath, dbPath);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to restore database");

                foreach (var suffix in movedSuffixes)
                {
                    _diskProvider.MoveFile(dbPath + PreRestoreSuffix + suffix, dbPath + suffix, true);
                }

                throw;
            }
        }

        public void HandleAsync(ApplicationStartedEvent message)
        {
            var dbPath = _appFolderInfo.GetDatabase();

            foreach (var suffix in DatabaseFileSuffixes)
            {
                _diskProvider.DeleteFile(dbPath + PreRestoreSuffix + suffix);
            }
        }
    }
}
