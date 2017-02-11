using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(126)]
    public class add_custom_filters : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("CustomFilters")
                  .WithColumn("Type").AsString().NotNullable()
                  .WithColumn("Label").AsString().NotNullable()
                  .WithColumn("Filters").AsString().NotNullable();
        }
    }
}
