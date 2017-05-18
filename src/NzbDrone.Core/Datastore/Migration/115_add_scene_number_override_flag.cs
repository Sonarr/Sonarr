using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(115)]
    public class add_scene_number_override_flag : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series").AddColumn("IgnoreSceneNumbering").AsBoolean().Nullable();
            Execute.Sql("UPDATE Series SET IgnoreSceneNumbering = 0");
        }
    }
}
