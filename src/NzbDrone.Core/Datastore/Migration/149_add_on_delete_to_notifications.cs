using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(149)]
    public class add_on_delete_to_notifications : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications").AddColumn("OnSeriesDelete").AsBoolean().WithDefaultValue(false);
            Alter.Table("Notifications").AddColumn("OnEpisodeFileDelete").AsBoolean().WithDefaultValue(false);
        }
    }
}
