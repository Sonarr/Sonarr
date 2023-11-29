using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(197)]
    public class list_add_missing_search : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("ImportLists").AddColumn("SearchForMissingEpisodes").AsBoolean().NotNullable().WithDefaultValue(true);
        }
    }
}
