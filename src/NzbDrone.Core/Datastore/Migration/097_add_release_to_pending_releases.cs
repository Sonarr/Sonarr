using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(97)]
    public class add_reason_to_pending_releases : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("PendingReleases").AddColumn("Reason").AsInt32().WithDefaultValue(0);
        }
    }
}
