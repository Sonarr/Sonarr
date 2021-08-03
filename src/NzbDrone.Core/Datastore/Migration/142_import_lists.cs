using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(142)]
    public class import_lists : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("ImportLists")
                  .WithColumn("Name").AsString().Unique()
                  .WithColumn("Implementation").AsString()
                  .WithColumn("Settings").AsString().Nullable()
                  .WithColumn("ConfigContract").AsString().Nullable()
                  .WithColumn("EnableAutomaticAdd").AsBoolean().Nullable()
                  .WithColumn("RootFolderPath").AsString()
                  .WithColumn("ShouldMonitor").AsInt32()
                  .WithColumn("QualityProfileId").AsInt32()
                  .WithColumn("LanguageProfileId").AsInt32()
                  .WithColumn("Tags").AsString().Nullable();

            Create.TableForModel("ImportListStatus")
                  .WithColumn("ProviderId").AsInt32().NotNullable().Unique()
                  .WithColumn("InitialFailure").AsDateTime().Nullable()
                  .WithColumn("MostRecentFailure").AsDateTime().Nullable()
                  .WithColumn("EscalationLevel").AsInt32().NotNullable()
                  .WithColumn("DisabledTill").AsDateTime().Nullable()
                  .WithColumn("LastSyncListInfo").AsString().Nullable();

            Create.TableForModel("ImportListExclusions")
                  .WithColumn("TvdbId").AsString().NotNullable().Unique()
                  .WithColumn("Title").AsString().NotNullable();
        }
    }
}
