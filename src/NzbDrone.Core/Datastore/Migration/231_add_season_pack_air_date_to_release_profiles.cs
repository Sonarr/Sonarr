using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration;

[Migration(231)]
public class add_season_pack_air_date_to_release_profiles : NzbDroneMigrationBase
{
    protected override void MainDbUpgrade()
    {
        Alter.Table("ReleaseProfiles").AddColumn("AllowSeasonPackWithoutAllEpisodesAired").AsBoolean().WithDefaultValue(false);
    }
}
