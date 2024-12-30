using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(82)]
    public class add_fanzub_settings : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Update.Table("Indexers").Set(new { ConfigContract = "FanzubSettings" }).Where(new { Implementation = "Fanzub", ConfigContract = "NullConfig" });
        }
    }
}
