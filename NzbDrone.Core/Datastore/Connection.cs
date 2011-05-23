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


        public static String MainConnectionString
        {
            get
            {
                return String.Format("Data Source={0};Version=3;", Path.Combine(AppDataPath.FullName, "nzbdrone.db"));
            }
        }

        public static String LogConnectionString
        {
            get
            {
                return String.Format("Data Source={0};Version=3;", Path.Combine(AppDataPath.FullName, "log.db"));
            }
        }


        private static IDataProvider _mainDataProvider;
        public static IDataProvider MainDataProvider
        {
            get
            {
                if (_mainDataProvider == null)
                {
                    _mainDataProvider = ProviderFactory.GetProvider(Connection.MainConnectionString, "System.Data.SQLite");
                }
                return _mainDataProvider;
            }

        }

        private static IDataProvider _logDataProvider;
        public static IDataProvider LogDataProvider
        {
            get
            {
                if (_logDataProvider == null)
                {
                    _logDataProvider = ProviderFactory.GetProvider(Connection.LogConnectionString, "System.Data.SQLite");
                }
                return _logDataProvider;
            }

        }


        private static SimpleRepository _mainDataRepository;
        public static SimpleRepository MainDataRepository
        {
            get
            {
                if (_mainDataRepository == null)
                {
                    _mainDataRepository = new SimpleRepository(MainDataProvider, SimpleRepositoryOptions.RunMigrations);
                }

                return _mainDataRepository;
            }

        }

        private static SimpleRepository _logDataRepository;
        public static SimpleRepository LogDataRepository
        {
            get
            {
                if (_logDataRepository == null)
                {
                    _logDataRepository = new SimpleRepository(LogDataProvider, SimpleRepositoryOptions.RunMigrations);
                }
                return _logDataRepository;
            }

        }



    }
}
