using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Datastore;
using System.Data;

namespace NzbDrone.Core.Backup
{
    public interface IMakeDatabaseBackup
    {
        void BackupDatabase(IDatabase database, string targetDirectory);
    }

    public class MakeDatabaseBackup : IMakeDatabaseBackup
    {
        private readonly Logger _logger;

        public MakeDatabaseBackup(Logger logger)
        {
            _logger = logger;
        }

        public void BackupDatabase(IDatabase database, string targetDirectory)
        {
            var sourceConnectionString = database.GetDataMapper().ConnectionString;
            var backupConnectionStringBuilder = new SQLiteConnectionStringBuilder(sourceConnectionString);

            backupConnectionStringBuilder.DataSource = Path.Combine(targetDirectory, Path.GetFileName(backupConnectionStringBuilder.DataSource));

            using (var sourceConnection = (SQLiteConnection)SQLiteFactory.Instance.CreateConnection())
            using (var backupConnection = (SQLiteConnection)SQLiteFactory.Instance.CreateConnection())
            {
                sourceConnection.ConnectionString = sourceConnectionString;
                backupConnection.ConnectionString = backupConnectionStringBuilder.ToString();

                sourceConnection.Open();
                backupConnection.Open();

                sourceConnection.BackupDatabase(backupConnection, "main", "main", -1, null, 500);

                // Make sure there are no lingering connections so the wal gets truncated.
                SQLiteConnection.ClearAllPools();
            }

            var backupWalPath = backupConnectionStringBuilder.DataSource + "-wal";
            if (backupConnectionStringBuilder.JournalMode == SQLiteJournalModeEnum.Wal && !File.Exists(backupWalPath))
            {
                // Make sure the wal gets created in the backup so users are less likely to make an error during restore.
                File.WriteAllBytes(backupWalPath, new byte[0]);
            }

            var backupJournalPath = backupConnectionStringBuilder.DataSource + "-journal";
            if (backupConnectionStringBuilder.JournalMode != SQLiteJournalModeEnum.Wal && !File.Exists(backupJournalPath))
            {
                // Make sure the journal gets created in the backup so users are less likely to make an error during restore.
                File.WriteAllBytes(backupJournalPath, new byte[0]);
            }
        }
    }
}
