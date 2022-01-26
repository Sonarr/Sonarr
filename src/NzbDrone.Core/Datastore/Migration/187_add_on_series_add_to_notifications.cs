using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(187)]
    public class add_on_series_add_to_notifications : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications").AddColumn("OnSeriesAdd").AsBoolean().WithDefaultValue(false);
        }
    }
}
