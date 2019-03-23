using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(131)]
    public class indexer_priority_column : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Indexers").AddColumn("Priority").AsInt32().Nullable();

            Execute.Sql("UPDATE Indexers SET Priority = 0");

            Alter.Table("Indexers").AlterColumn("Priority").AsInt32().NotNullable();
        }
    }
}
