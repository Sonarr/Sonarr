using System.Data.Common;
using System.Data.SqlServerCe;
using System.IO;

namespace NzbDrone.SqlCe
{
    public class SqlCeProxy
    {
        public void EnsureDatabase(string constr)
        {
            var connection = new SqlCeConnection(constr);

            if (!File.Exists(connection.Database))
            {
                var engine = new SqlCeEngine(constr);
                engine.CreateDatabase();
            }
        }

        public DbProviderFactory GetSqlCeProviderFactory()
        {
            return new SqlCeProviderFactory();
        }
    }
}
