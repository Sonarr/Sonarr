using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(94)]
    public class add_tvmazeid : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series").AddColumn("TvMazeId").AsInt32().WithDefaultValue(0);
            Create.Index().OnTable("Series").OnColumn("TvMazeId");
        }
    }
}
