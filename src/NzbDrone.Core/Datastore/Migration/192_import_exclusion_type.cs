using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(192)]
    public class import_exclusion_type : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            IfDatabase(ProcessorIdConstants.SQLite).Alter.Table("ImportListExclusions").AlterColumn("TvdbId").AsInt32();

            // PG cannot autocast varchar to integer
            IfDatabase(ProcessorIdConstants.PostgreSQL).Execute.Sql("ALTER TABLE \"ImportListExclusions\" ALTER COLUMN \"TvdbId\" TYPE INTEGER USING \"TvdbId\"::integer");
        }
    }
}
