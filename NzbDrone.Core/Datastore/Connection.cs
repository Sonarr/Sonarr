using System;
using System.Data;
using System.Data.SQLite;
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
        }


        public static string GetConnectionString(string path)
        {
            return String.Format("Data Source={0};Version=3;Cache Size=30000;", path);
        }

        public static String MainConnectionString
        {
            get
            {
                return GetConnectionString(Path.Combine(AppDataPath.FullName, "nzbdrone.db"));
            }
        }

        public static String LogConnectionString
        {
            get
            {
                return GetConnectionString(Path.Combine(AppDataPath.FullName, "log.db"));
            }
        }

        
        public static IDatabase GetPetaPocoDb(string connectionString)
        {
            MigrationsHelper.Run(connectionString, true);

            var profileConnection = ProfiledDbConnection.Get(new SQLiteConnection(connectionString));

            Database.Mapper = new CustomeMapper();
            var db = new Database(profileConnection);

            if (profileConnection.State != ConnectionState.Open)
                profileConnection.Open();

            return db;
        }

    }
}
