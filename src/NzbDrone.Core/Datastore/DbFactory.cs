using System;
using System.Data.Common;
using System.Data.SQLite;
using System.Net.Sockets;
using System.Threading;
using NLog;
using Npgsql;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Exceptions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore
{
    public interface IDbFactory
    {
        IDatabase Create(MigrationType migrationType = MigrationType.Main);
        IDatabase Create(MigrationContext migrationContext);
    }

    public class DbFactory : IDbFactory
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(DbFactory));
        private readonly IMigrationController _migrationController;
        private readonly IConnectionStringFactory _connectionStringFactory;
        private readonly IDiskProvider _diskProvider;
        private readonly IRestoreDatabase _restoreDatabaseService;

        static DbFactory()
        {
            InitializeEnvironment();

            TableMapping.Map();
        }

        private static void InitializeEnvironment()
        {
            // Speed up sqlite3 initialization since we don't use the config file and can't rely on preloading.
            Environment.SetEnvironmentVariable("No_Expand", "true");
            Environment.SetEnvironmentVariable("No_SQLiteXmlConfigFile", "true");
            Environment.SetEnvironmentVariable("No_PreLoadSQLite", "true");
            Environment.SetEnvironmentVariable("No_SQLiteFunctions", "true");
        }

        public DbFactory(IMigrationController migrationController,
                         IConnectionStringFactory connectionStringFactory,
                         IDiskProvider diskProvider,
                         IRestoreDatabase restoreDatabaseService)
        {
            _migrationController = migrationController;
            _connectionStringFactory = connectionStringFactory;
            _diskProvider = diskProvider;
            _restoreDatabaseService = restoreDatabaseService;
        }

        public IDatabase Create(MigrationType migrationType = MigrationType.Main)
        {
            return Create(new MigrationContext(migrationType));
        }

        public IDatabase Create(MigrationContext migrationContext)
        {
            DatabaseConnectionInfo connectionInfo;

            switch (migrationContext.MigrationType)
            {
                case MigrationType.Main:
                    {
                        connectionInfo = _connectionStringFactory.MainDbConnection;
                        CreateMain(connectionInfo.ConnectionString, migrationContext, connectionInfo.DatabaseType);

                        break;
                    }

                case MigrationType.Log:
                    {
                        connectionInfo = _connectionStringFactory.LogDbConnection;
                        CreateLog(connectionInfo.ConnectionString, migrationContext, connectionInfo.DatabaseType);

                        break;
                    }

                default:
                    {
                        throw new ArgumentException("Invalid MigrationType");
                    }
            }

            var db = new Database(migrationContext.MigrationType.ToString(), () =>
            {
                DbConnection conn;

                if (connectionInfo.DatabaseType == DatabaseType.SQLite)
                {
                    conn = SQLiteFactory.Instance.CreateConnection();
                    conn.ConnectionString = connectionInfo.ConnectionString;
                }
                else
                {
                    conn = new NpgsqlConnection(connectionInfo.ConnectionString);
                }

                conn.Open();
                return conn;
            });

            return db;
        }

        private void CreateMain(string connectionString, MigrationContext migrationContext, DatabaseType databaseType)
        {
            try
            {
                _restoreDatabaseService.Restore();
                _migrationController.Migrate(connectionString, migrationContext, databaseType);
            }
            catch (SQLiteException e)
            {
                var fileName = _connectionStringFactory.GetDatabasePath(connectionString);

                if (OsInfo.IsOsx)
                {
                    throw new CorruptDatabaseException("Database file: {0} is corrupt, restore from backup if available. See: https://wiki.servarr.com/sonarr/faq#i-use-sonarr-on-a-mac-and-it-suddenly-stopped-working-what-happened", e, fileName);
                }

                throw new CorruptDatabaseException("Database file: {0} is corrupt, restore from backup if available. See: https://wiki.servarr.com/sonarr/faq#i-am-getting-an-error-database-disk-image-is-malformed", e, fileName);
            }
            catch (NpgsqlException e)
            {
                if (e.InnerException is SocketException)
                {
                    var retryCount = 3;

                    while (true)
                    {
                        Logger.Error(e, "Failure to connect to Postgres DB, {0} retries remaining", retryCount);

                        Thread.Sleep(5000);

                        try
                        {
                            _migrationController.Migrate(connectionString, migrationContext, databaseType);
                            return;
                        }
                        catch (Exception ex)
                        {
                            if (--retryCount > 0)
                            {
                                continue;
                            }

                            throw new SonarrStartupException(ex, "Error creating main database");
                        }
                    }
                }
                else
                {
                    throw new SonarrStartupException(e, "Error creating main database");
                }
            }
            catch (Exception e)
            {
                throw new SonarrStartupException(e, "Error creating main database");
            }
        }

        private void CreateLog(string connectionString, MigrationContext migrationContext, DatabaseType databaseType)
        {
            try
            {
                _migrationController.Migrate(connectionString, migrationContext, databaseType);
            }
            catch (SQLiteException e)
            {
                var fileName = _connectionStringFactory.GetDatabasePath(connectionString);

                Logger.Error(e, "Logging database is corrupt, attempting to recreate it automatically");

                try
                {
                    _diskProvider.DeleteFile(fileName + "-shm");
                    _diskProvider.DeleteFile(fileName + "-wal");
                    _diskProvider.DeleteFile(fileName + "-journal");
                    _diskProvider.DeleteFile(fileName);
                }
                catch (Exception)
                {
                    Logger.Error("Unable to recreate logging database automatically. It will need to be removed manually.");
                }

                _migrationController.Migrate(connectionString, migrationContext, databaseType);
            }
            catch (Exception e)
            {
                throw new SonarrStartupException(e, "Error creating log database");
            }
        }
    }
}
