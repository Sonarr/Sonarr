using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(54)]
    public class rename_profiles : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Rename.Table("QualityProfiles").To("Profiles");

            Alter.Table("Profiles").AddColumn("Language").AsInt32().Nullable();
            Alter.Table("Profiles").AddColumn("GrabDelay").AsInt32().Nullable();
            Alter.Table("Profiles").AddColumn("GrabDelayMode").AsInt32().Nullable();
            Execute.Sql("UPDATE Profiles SET Language = 1, GrabDelay = 0, GrabDelayMode = 0");

            // Rename QualityProfileId in Series
            Alter.Table("Series").AddColumn("ProfileId").AsInt32().Nullable();
            Execute.Sql("UPDATE Series SET ProfileId = QualityProfileId");

            // Add HeldReleases
            Create.TableForModel("PendingReleases")
                  .WithColumn("SeriesId").AsInt32()
                  .WithColumn("Title").AsString()
                  .WithColumn("Added").AsDateTime()
                  .WithColumn("ParsedEpisodeInfo").AsString()
                  .WithColumn("Release").AsString();
        }
    }
}
