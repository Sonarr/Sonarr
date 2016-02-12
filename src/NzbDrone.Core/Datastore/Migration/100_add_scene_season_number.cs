using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(100)]
    public class add_scene_season_number : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("SceneMappings").AlterColumn("SeasonNumber").AsInt32().Nullable();
            Alter.Table("SceneMappings").AddColumn("SceneSeasonNumber").AsInt32().Nullable();
        }
    }
}
