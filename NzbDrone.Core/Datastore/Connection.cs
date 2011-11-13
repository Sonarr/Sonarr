using System;
using NzbDrone.Common;
using PetaPoco;

namespace NzbDrone.Core.Datastore
{
    public class Connection
    {
        private readonly EnviromentProvider _enviromentProvider;

        public Connection(EnviromentProvider enviromentProvider)
        {
            _enviromentProvider = enviromentProvider;
        }

        static Connection()
        {
            Database.Mapper = new CustomeMapper();
        }

        public String MainConnectionString
        {
            get
            {
                return GetConnectionString(_enviromentProvider.GetNzbDronoeDbFile());
            }
        }

        public String LogConnectionString
        {
            get
            {
               return GetConnectionString(_enviromentProvider.GetLogDbFileDbFile());
            }
        }

        public static string GetConnectionString(string path)
        {
            //return String.Format("Data Source={0};Version=3;Cache Size=30000;Pooling=true;Default Timeout=2", path);
            return String.Format("Data Source={0}", path);
        }

        public IDatabase GetMainPetaPocoDb(Boolean profiled = true)
        {
            return GetPetaPocoDb(MainConnectionString, profiled);
        }

        public IDatabase GetLogPetaPocoDb(Boolean profiled = true)
        {
            return GetPetaPocoDb(LogConnectionString, profiled);
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
