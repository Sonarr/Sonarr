using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Text;
using MvcMiniProfiler;
using MvcMiniProfiler.Data;

namespace NzbDrone.Core.Datastore
{
    class PetaDbProviderFactory : DbProviderFactory
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
