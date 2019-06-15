using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(131)]
    public class add_on_health_check_failed_to_notifcations : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications").AddColumn("OnHealthCheckFailed").AsBoolean().Nullable();
        }
    }
}
