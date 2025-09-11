using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(221)]
    public class add_exclusion_tags_to_release_profiles : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("ReleaseProfiles").AddColumn("ExcludedTags").AsString().NotNullable().WithDefaultValue("[]");
        }
    }
}
