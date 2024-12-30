using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(104)]
    public class remove_kickass : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.FromTable("Indexers").Row(new { Implementation = "KickassTorrents" });
        }
    }
}
