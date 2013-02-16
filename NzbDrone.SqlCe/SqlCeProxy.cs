using System.Data.Common;
using System.Data.SqlServerCe;
using System.IO;

namespace NzbDrone.SqlCe
{
    public class SqlCeProxy
    {
        public void EnsureDatabase(string connectionString)
        {
            var connection = new SqlCeConnection(connectionString);

            if (!File.Exists(connection.Database))
            {
                var engine = new SqlCeEngine(connectionString);
                engine.CreateDatabase();
            }
        }

        public DbProviderFactory GetSqlCeProviderFactory()
        {
            return new SqlCeProviderFactory();
        }
    }
}
