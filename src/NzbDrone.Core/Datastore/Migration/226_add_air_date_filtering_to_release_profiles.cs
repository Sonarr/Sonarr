using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration;

[Migration(226)]
public class add_air_date_filtering_to_release_profiles : NzbDroneMigrationBase
{
    protected override void MainDbUpgrade()
    {
        Alter.Table("ReleaseProfiles").AddColumn("AirDateRestriction").AsBoolean().WithDefaultValue(false);
        Alter.Table("ReleaseProfiles").AddColumn("AirDateGracePeriod").AsInt32().WithDefaultValue(0);
    }
}
