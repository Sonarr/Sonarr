using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
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
