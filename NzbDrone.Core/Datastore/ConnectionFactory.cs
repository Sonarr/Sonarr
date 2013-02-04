using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlServerCe;
using System.IO;
using NLog;
using NzbDrone.Common;
using PetaPoco;

namespace NzbDrone.Core.Datastore
{
    public class ConnectionFactory
    {
        private readonly EnvironmentProvider _environmentProvider;
        private static readonly Logger logger = LogManager.GetLogger("ConnectionFactory");

        static ConnectionFactory()
        {
            Database.Mapper = new CustomeMapper();

            var dataSet = (System.Data.DataSet)ConfigurationManager.GetSection("system.data");
            dataSet.Tables[0].Rows.Add("Microsoft SQL Server Compact Data Provider 4.0"
            , "System.Data.SqlServerCe.4.0"
            , ".NET Framework Data Provider for Microsoft SQL Server Compact"
            , "System.Data.SqlServerCe.SqlCeProviderFactory, System.Data.SqlServerCe, Version=4.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
        }

        public ConnectionFactory(EnvironmentProvider environmentProvider)
        {
            _environmentProvider = environmentProvider;
        }

        public String MainConnectionString
        {
            get
            {
                return GetConnectionString(_environmentProvider.GetSqlCeMainDbPath());
            }
        }

        public String LogConnectionString
        {
            get
            {
                return GetConnectionString(_environmentProvider.GetSqlCeLogDbPath());
            }
        }

        public static string GetConnectionString(string path)
        {
            return String.Format("Data Source=\"{0}\"; Max Database Size = 512;", path);
        }

        public IDatabase GetMainPetaPocoDb(Boolean profiled = true)
        {
            return GetPetaPocoDb(MainConnectionString, profiled);
        }

        public IDatabase GetLogPetaPocoDb(Boolean profiled = true)
        {
            return GetPetaPocoDb(LogConnectionString, profiled);
        }


        static readonly HashSet<String> initilized = new HashSet<string>();


        public static IDatabase GetPetaPocoDb(string connectionString, Boolean profiled = true)
        {
            lock (initilized)
            {
                if (!initilized.Contains(connectionString))
                {
                    VerifyDatabase(connectionString);
                    MigrationsHelper.Run(connectionString, true);
                    initilized.Add(connectionString);
                }
            }

            var factory = new DbProviderFactory
                              {
                                  IsProfiled = profiled
                              };

            var db = new Database(connectionString, factory, Database.DBType.SqlServerCE)
                         {
                             KeepConnectionAlive = true,
                             ForceDateTimesToUtc = false,
                         };

            return db;
        }

        private static void VerifyDatabase(string connectionString)
        {
            logger.Debug("Verifying database {0}", connectionString);

            var sqlConnection = new SqlCeConnection(connectionString);

            if (!File.Exists(sqlConnection.Database))
            {
                logger.Debug("database file doesn't exist. {0}", sqlConnection.Database);
                return;
            }

            using (var sqlEngine = new SqlCeEngine(connectionString))
            {

                if (sqlEngine.Verify(VerifyOption.Default))
                {
                    logger.Debug("Database integrity verified.");
                }
                else
                {
                    logger.Error("Database verification failed.");
                    RepairDatabase(connectionString);
                }
            }
        }

        private static void RepairDatabase(string connectionString)
        {
            logger.Info("Attempting to repair database: {0}", connectionString);
            using (var sqlEngine = new SqlCeEngine(connectionString))
            {
                try
                {
                    sqlEngine.Repair(connectionString, RepairOption.RecoverAllOrFail);
                    logger.Info("Recovery was successful without any data loss {0}", connectionString);
                }
                catch (SqlCeException e)
                {
                    if (e.Message.Contains("file sharing violation"))
                    {
                        logger.WarnException("file is in use. skipping.", e);
                        return;
                    }
                    logger.WarnException(
                                         "Safe recovery failed. will attempts a more aggressive strategy. might cause loss of data.",
                                         e);
                    sqlEngine.Repair(connectionString, RepairOption.DeleteCorruptedRows);
                    logger.Warn("Database was recovered. some data might have been lost");

                    //TODO: do db cleanup to avoid broken relationships.
                }
            }
        }
    }
}
