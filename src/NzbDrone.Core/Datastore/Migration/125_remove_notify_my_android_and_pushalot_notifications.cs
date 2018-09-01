using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(125)]
    public class remove_notify_my_android_and_pushalot_notifications : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.FromTable("Notifications").Row(new { Implementation = "NotifyMyAndroid" });
            Delete.FromTable("Notifications").Row(new { Implementation = "Pushalot" });
        }
    }
}
