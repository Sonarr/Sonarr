using System;
using System.Data.SQLite;
using System.IO;
using MvcMiniProfiler.Data;
using PetaPoco;
using SubSonic.DataProviders;
using SubSonic.DataProviders.SQLite;
using SubSonic.Repository;

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

        public static IDataProvider GetDataProvider(string connectionString)
        {
            return new ProfiledSQLiteProvider(connectionString, "System.Data.SQLite");
        }

        public static IRepository CreateSimpleRepository(IDataProvider dataProvider)
        {
            return new SimpleRepository(dataProvider, SimpleRepositoryOptions.RunMigrations);
        }

        public static IRepository CreateSimpleRepository(string connectionString)
        {
            return new SimpleRepository(GetDataProvider(connectionString), SimpleRepositoryOptions.RunMigrations);
        }

        public static IDatabase GetPetaPocoDb(string connectionString)
        {
            var profileConnection = ProfiledDbConnection.Get(new SQLiteConnection(connectionString));
            PetaPoco.Database.Mapper = new CustomeMapper();
            var db = new PetaPoco.Database(profileConnection);
            db.OpenSharedConnection();

            return db;
        }

    }


    public class ProfiledSQLiteProvider : SQLiteProvider
    {
        public ProfiledSQLiteProvider(string connectionString, string providerName)
            : base(connectionString, providerName)
        {

        }

        public override System.Data.Common.DbConnection CreateConnection(string connectionString)
        {
            return ProfiledDbConnection.Get(base.CreateConnection(connectionString));

        }
    }
}
