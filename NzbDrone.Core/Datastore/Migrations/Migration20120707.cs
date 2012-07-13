using System.Data;
using Migrator.Framework;

namespace NzbDrone.Core.Datastore.Migrations
{

    [Migration(20120707)]
    public class Migration20120707 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.AddTable("MetadataDefinitions", new[]
                                            {
                                                new Column("Id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                                                new Column("Enable", DbType.Boolean, ColumnProperty.NotNull), 
                                                new Column("MetadataProviderType", DbType.String, ColumnProperty.NotNull), 
                                                new Column("Name", DbType.String, ColumnProperty.NotNull)
                                            });
        }
    }
}