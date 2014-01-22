using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(37)]
    public class add_metadata_consumers : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("MetadataConsumers")
                  .WithColumn("Enable").AsBoolean().NotNullable()
                  .WithColumn("Name").AsString().NotNullable()
                  .WithColumn("Implementation").AsString().NotNullable()
                  .WithColumn("Settings").AsString().NotNullable()
                  .WithColumn("ConfigContract").AsString().NotNullable();
        }
    }
}
