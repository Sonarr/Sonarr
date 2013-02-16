using System.Data.Common;
using System.Data.SqlServerCe;
using System.IO;

namespace NzbDrone.SqlCe
{
    public class SqlCeProxy
    {
        public SqlCeConnection EnsureDatabase(string connectionString)
        {
            var connection = new SqlCeConnection(connectionString);

            if (!File.Exists(connection.Database))
            {
                var engine = new SqlCeEngine(connectionString);
                engine.CreateDatabase();
            }

            return connection;
        }

        public DbProviderFactory GetSqlCeProviderFactory()
        {
            return new SqlCeProviderFactory();
        }
    }
}
