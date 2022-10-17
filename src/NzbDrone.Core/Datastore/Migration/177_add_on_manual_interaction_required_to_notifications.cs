using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(177)]
    public class add_on_manual_interaction_required_to_notifications : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications").AddColumn("OnManualInteractionRequired").AsBoolean().WithDefaultValue(false);
        }
    }
}
