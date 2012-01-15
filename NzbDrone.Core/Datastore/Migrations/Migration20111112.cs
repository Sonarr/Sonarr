using System;
using System.Data;
using Migrator.Framework;

namespace NzbDrone.Core.Datastore.Migrations
{

    [Migration(20111112)]
    public class Migration2011112 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.AddTable("NewznabDefinitions", new[]
                                            {
                                                new Column("Id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                                                new Column("Enable", DbType.Boolean, ColumnProperty.NotNull),
                                                new Column("Name", DbType.String, ColumnProperty.Null),
                                                new Column("Url", DbType.String, ColumnProperty.Null),
                                                new Column("ApiKey", DbType.String, ColumnProperty.Null)
                                            });
        }
    }
}