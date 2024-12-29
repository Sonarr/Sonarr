using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(16)]
    public class updated_imported_history_item : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"History\" SET \"Data\" = replace( \"Data\", '\"Path\"', '\"ImportedPath\"' ) WHERE \"EventType\" = 3");
        }
    }
}
