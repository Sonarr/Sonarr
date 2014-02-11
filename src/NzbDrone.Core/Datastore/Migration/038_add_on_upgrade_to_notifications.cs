using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(38)]
    public class add_on_upgrade_to_notifications : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications").AddColumn("OnUpgrade").AsBoolean().Nullable();

            Execute.Sql("UPDATE Notifications SET OnUpgrade = OnDownload");
        }
    }
}
