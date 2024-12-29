using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(142)]
    public class import_lists : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            MigrationExtension.TableForModel(Create, "ImportLists")
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

            MigrationExtension.TableForModel(Create, "ImportListStatus")
                  .WithColumn("ProviderId").AsInt32().NotNullable().Unique()
                  .WithColumn("InitialFailure").AsDateTime().Nullable()
                  .WithColumn("MostRecentFailure").AsDateTime().Nullable()
                  .WithColumn("EscalationLevel").AsInt32().NotNullable()
                  .WithColumn("DisabledTill").AsDateTime().Nullable()
                  .WithColumn("LastSyncListInfo").AsString().Nullable();

            MigrationExtension.TableForModel(Create, "ImportListExclusions")
                  .WithColumn("TvdbId").AsString().NotNullable().Unique()
                  .WithColumn("Title").AsString().NotNullable();
        }
    }
}
