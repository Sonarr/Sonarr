using System;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlServerCe;
using NzbDrone.Common;
using NzbDrone.Core.Instrumentation;
using PetaPoco;

namespace NzbDrone.Core.Datastore
{
    public class Connection
    {
        private readonly EnvironmentProvider _environmentProvider;

        static Connection()
        {
            Database.Mapper = new CustomeMapper();

            var dataSet = ConfigurationManager.GetSection("system.data") as System.Data.DataSet;
            dataSet.Tables[0].Rows.Add("Microsoft SQL Server Compact Data Provider 4.0"
            , "System.Data.SqlServerCe.4.0"
            , ".NET Framework Data Provider for Microsoft SQL Server Compact"
            , "System.Data.SqlServerCe.SqlCeProviderFactory, System.Data.SqlServerCe, Version=4.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
        }

        public Connection(EnvironmentProvider environmentProvider)
        {
            _environmentProvider = environmentProvider;
        }
        
        public String MainConnectionString
        {
            get
            {
                return GetConnectionString(_environmentProvider.GetNzbDroneDbFile());
            }
        }

        public String LogConnectionString
        {
            get
            {
                return GetConnectionString(_environmentProvider.GetLogDbFileDbFile());
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

        public static IDatabase GetPetaPocoDb(string connectionString, Boolean profiled = true)
        {
            MigrationsHelper.Run(connectionString, true);

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
    }
}
