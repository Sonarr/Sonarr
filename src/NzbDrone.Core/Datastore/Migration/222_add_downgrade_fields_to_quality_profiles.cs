using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(222)]
    public class add_downgrade_fields_to_quality_profiles : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("QualityProfiles")
                .AddColumn("DowngradeAllowed").AsBoolean().WithDefaultValue(false);

            Alter.Table("QualityProfiles")
                .AddColumn("DowngradeToProfileId").AsInt32().Nullable();

            Alter.Table("QualityProfiles")
                .AddColumn("DowngradeAfterDays").AsInt32().Nullable();
        }
    }
}

