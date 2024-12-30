using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
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
