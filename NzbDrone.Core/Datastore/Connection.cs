using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.IO;
using MvcMiniProfiler.Data;
using NzbDrone.Common;
using NzbDrone.Core.Providers;
using PetaPoco;

namespace NzbDrone.Core.Datastore
{
    public static class Connection
    {
        private static EnviromentProvider _enviromentProvider = new EnviromentProvider();

        static Connection()
        {
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
                return GetConnectionString(Path.Combine(_enviromentProvider.AppDataPath, "nzbdrone.sdf"));
            }
        }

        public static String LogConnectionString
        {
            get
            {
                return GetConnectionString(Path.Combine(_enviromentProvider.AppDataPath, "log.sdf"));
            }
        }


        public static IDatabase GetPetaPocoDb(string connectionString, Boolean profiled = true)
        {
            MigrationsHelper.Run(connectionString, true);

            var factory = new PetaDbProviderFactory
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

    }
}
