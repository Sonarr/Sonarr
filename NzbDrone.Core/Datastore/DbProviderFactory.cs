using System;
using System.Data.Common;
using System.Data.SqlServerCe;
using MvcMiniProfiler;
using MvcMiniProfiler.Data;

namespace NzbDrone.Core.Datastore
{
    class DbProviderFactory : System.Data.Common.DbProviderFactory
    {
        public Boolean IsProfiled { get; set; }

        public override DbConnection CreateConnection()
        {
            var sqliteConnection = new SqlCeConnection();
            DbConnection connection = sqliteConnection;

            if (IsProfiled)
            {
                connection = new ProfiledDbConnection(sqliteConnection, MiniProfiler.Current);
            }

            return connection;
        }
    }
}
