using System.Data.Common;
using System.Data.SQLite;
using FluentMigrator.Runner.Processors;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public class MigrationDbFactory : DbFactoryBase
    {
        protected override DbProviderFactory CreateFactory()
        {
            return SQLiteFactory.Instance;
        }
    }
}