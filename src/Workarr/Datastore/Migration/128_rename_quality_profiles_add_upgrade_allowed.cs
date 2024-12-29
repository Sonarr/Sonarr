using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(128)]
    public class rename_quality_profiles_add_upgrade_allowed : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Rename.Table("Profiles").To("QualityProfiles");

            Alter.Table("QualityProfiles").AddColumn("UpgradeAllowed").AsBoolean().Nullable();
            Alter.Table("LanguageProfiles").AddColumn("UpgradeAllowed").AsBoolean().Nullable();

            // Set upgrade allowed for existing profiles (default will be false for new profiles)
            Update.Table("QualityProfiles").Set(new { UpgradeAllowed = true }).AllRows();
            Update.Table("LanguageProfiles").Set(new { UpgradeAllowed = true }).AllRows();

            Rename.Column("ProfileId").OnTable("Series").To("QualityProfileId");
        }
    }
}
