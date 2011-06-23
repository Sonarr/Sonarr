using System;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Data.SqlServerCe;
using System.IO;
using MvcMiniProfiler.Data;
using PetaPoco;

namespace NzbDrone.Core.Datastore
{
    public static class Connection
    {
        private static readonly DirectoryInfo AppDataPath = new DirectoryInfo(Path.Combine(CentralDispatch.AppPath, "App_Data"));

        static Connection()
        {
            if (!AppDataPath.Exists) AppDataPath.Create();
            Database.Mapper = new CustomeMapper();
        }


        public static string GetConnectionString(string path)
        {
            //return String.Format("Data Source={0};Version=3;Cache Size=30000;Pooling=true;Default Timeout=2", path);
            return String.Format("Data Source={0}", path);
        }

        public static String MainConnectionString
        {
            get
            {
                return GetConnectionString(Path.Combine(AppDataPath.FullName, "nzbdrone.sdf"));
            }
        }

        public static String LogConnectionString
        {
            get
            {
                return GetConnectionString(Path.Combine(AppDataPath.FullName, "log.sdf"));
            }
        }


        public static IDatabase GetPetaPocoDb(string connectionString, Boolean profiled = true)
        {
            MigrationsHelper.Run(connectionString, true);
            var sqliteConnection = new SqlCeConnection(connectionString);
            DbConnection connection = sqliteConnection;

            if (profiled)
            {
                connection = ProfiledDbConnection.Get(sqliteConnection);
            }

            var db = new Database(connection);
            db.ForceDateTimesToUtc = false;

            if (connection.State != ConnectionState.Open)
                connection.Open();

            return db;
        }

    }
}
