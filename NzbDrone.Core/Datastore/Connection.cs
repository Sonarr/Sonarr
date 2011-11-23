using System;
using System.Configuration;
using System.Data.Common;
using System.Data.EntityClient;
using System.Data.SqlServerCe;
using MvcMiniProfiler;
using NzbDrone.Common;
using NzbDrone.Core.Instrumentation;
using PetaPoco;

namespace NzbDrone.Core.Datastore
{
    public class Connection
    {
        private readonly EnviromentProvider _enviromentProvider;


        public static void InitiFacotry()
        {

                var dataSet = ConfigurationManager.GetSection("system.data") as System.Data.DataSet;
                dataSet.Tables[0].Rows.Add("Microsoft SQL Server Compact Data Provider 4.0"
                , "System.Data.SqlServerCe.4.0"
                , ".NET Framework Data Provider for Microsoft SQL Server Compact"
                , "System.Data.SqlServerCe.SqlCeProviderFactory, System.Data.SqlServerCe, Version=4.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");   
        }

        public Connection(EnviromentProvider enviromentProvider)
        {
            _enviromentProvider = enviromentProvider;
        }

        static Connection()
        {
            Database.Mapper = new CustomeMapper();
            InitiFacotry();
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

        public LogDbContext GetLogEfContext()
        {
            return GetLogDbContext(LogConnectionString);
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

        public static LogDbContext GetLogDbContext(string connectionString)
        {
            MigrationsHelper.Run(connectionString, true);
            DbConnection connection = new SqlCeConnection(connectionString);
            return new LogDbContext(connection);
        }

    }
}
