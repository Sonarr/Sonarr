using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(135)]
    public class health_issue_notification : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications").AddColumn("OnHealthIssue").AsBoolean().WithDefaultValue(false);
            Alter.Table("Notifications").AddColumn("IncludeHealthWarnings").AsBoolean().WithDefaultValue(false);
        }
    }
}
