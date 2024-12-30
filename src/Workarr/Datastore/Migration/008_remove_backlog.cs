using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(8)]
    public class remove_backlog : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("BacklogSetting").FromTable("Series");
            Delete.Column("UseSceneName").FromTable("NamingConfig");
        }
    }
}
