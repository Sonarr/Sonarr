using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(136)]
    public class add_indexer_and_enabled_to_release_profiles : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("ReleaseProfiles").AddColumn("Enabled").AsBoolean().WithDefaultValue(true);
            Alter.Table("ReleaseProfiles").AddColumn("IndexerId").AsInt32().WithDefaultValue(0);
        }
    }
}
