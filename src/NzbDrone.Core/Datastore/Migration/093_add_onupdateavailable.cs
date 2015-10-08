using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(93)]
    public class add_onupdateavailable : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications").AddColumn("OnUpdateAvailable").AsBoolean().WithDefaultValue(false);
        }
    }
}
