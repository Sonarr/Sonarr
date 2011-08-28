using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;
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
                connection = ProfiledDbConnection.Get(sqliteConnection);
            }

            return connection;
        }
    }
}
