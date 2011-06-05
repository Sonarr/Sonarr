using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Migrator.Framework;
using Migrator.Providers.SQLite;

namespace NzbDrone.Core.Datastore
{
    class SqliteProvider
    {
        private readonly ITransformationProvider _dataBase;

        public SqliteProvider(string connectionString)
        {
            _dataBase = new SQLiteTransformationProvider(new SQLiteDialect(), connectionString);
        }


        public int GetPageSize()
        {
            return Convert.ToInt32(_dataBase.ExecuteScalar("PRAGMA cache_size"));
        }



    }
}
