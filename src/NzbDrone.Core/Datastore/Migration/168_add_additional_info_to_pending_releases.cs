using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(168)]
    public class add_additional_info_to_pending_releases : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("PendingReleases").AddColumn("AdditionalInfo").AsString().Nullable();
        }
    }
}
