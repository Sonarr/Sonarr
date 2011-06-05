using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SubSonic.DataProviders;
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
            return String.Format("Data Source={0};Version=3;", path);
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
            return ProviderFactory.GetProvider(connectionString, "System.Data.SQLite");
        }

        public static IRepository CreateSimpleRepository(IDataProvider dataProvider)
        {
            return new SimpleRepository(dataProvider, SimpleRepositoryOptions.RunMigrations);
        }

        public static IRepository CreateSimpleRepository(string connectionString)
        {
            return new SimpleRepository(GetDataProvider(connectionString), SimpleRepositoryOptions.RunMigrations);
        }

    }
}
