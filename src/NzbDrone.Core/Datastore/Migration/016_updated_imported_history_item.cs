using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(16)]
    public class updated_imported_history_item : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql(@"UPDATE HISTORY SET Data = replace( Data, '""Path""', '""ImportedPath""' ) WHERE EventType=3");
        }
    }
}
