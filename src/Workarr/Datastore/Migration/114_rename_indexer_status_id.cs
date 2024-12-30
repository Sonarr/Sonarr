using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
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
