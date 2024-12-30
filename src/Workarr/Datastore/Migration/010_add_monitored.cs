using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(10)]
    public class add_monitored : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Episodes").AddColumn("Monitored").AsBoolean().Nullable();
            Alter.Table("Seasons").AddColumn("Monitored").AsBoolean().Nullable();

            Update.Table("Episodes").Set(new { Monitored = true }).Where(new { Ignored = false });
            Update.Table("Episodes").Set(new { Monitored = false }).Where(new { Ignored = true });

            Update.Table("Seasons").Set(new { Monitored = true }).Where(new { Ignored = false });
            Update.Table("Seasons").Set(new { Monitored = false }).Where(new { Ignored = true });
        }
    }
}
