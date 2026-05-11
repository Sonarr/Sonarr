using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration;

[Migration(228)]
public class add_external_decisions : NzbDroneMigrationBase
{
    protected override void MainDbUpgrade()
    {
        Create.TableForModel("ExternalDecisions")
              .WithColumn("Enable").AsBoolean().NotNullable()
              .WithColumn("Name").AsString().NotNullable()
              .WithColumn("Implementation").AsString().NotNullable()
              .WithColumn("Settings").AsString().NotNullable()
              .WithColumn("ConfigContract").AsString().NotNullable()
              .WithColumn("DecisionType").AsInt32().NotNullable()
              .WithColumn("Priority").AsInt32().NotNullable().WithDefaultValue(25)
              .WithColumn("Tags").AsString().Nullable();

        Create.TableForModel("ExternalDecisionStatus")
              .WithColumn("ProviderId").AsInt32().NotNullable().Unique()
              .WithColumn("InitialFailure").AsDateTimeOffset().Nullable()
              .WithColumn("MostRecentFailure").AsDateTimeOffset().Nullable()
              .WithColumn("EscalationLevel").AsInt32().NotNullable()
              .WithColumn("DisabledTill").AsDateTimeOffset().Nullable();
    }
}
