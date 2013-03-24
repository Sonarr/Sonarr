using System;
using System.Data;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Sqlite;

namespace NzbDrone.Core.Datastore
{
    public interface IDbFactory
    {
        IDbConnection Create(string dbPath = null);
    }

    public class DbFactory : IDbFactory
    {
        private const string MemoryConnectionString = "Data Source=:memory:;Version=3;New=True;";

        static DbFactory()
        {
            OrmLiteConfig.DialectProvider = new SqliteOrmLiteDialectProvider();
        }

        public IDbConnection Create(string dbPath = null)
        {
            var connectionString = MemoryConnectionString;

            if (!string.IsNullOrWhiteSpace(dbPath))
            {
                connectionString = GetConnectionString(dbPath);
            }

            OrmLiteConfig.DialectProvider = new SqliteOrmLiteDialectProvider();
            var dbFactory = new OrmLiteConnectionFactory(connectionString);
            return dbFactory.Open();
        }

        private string GetConnectionString(string dbPath)
        {
            return String.Format("Data Source={0};Version=3;", dbPath);
        }
    }
}
