using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(114)]
    public class rename_indexer_status_id : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Rename.Column("IndexerId").OnTable("IndexerStatus").To("ProviderId");
        }
    }
}
