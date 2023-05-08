using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(150)]
    public class add_scene_mapping_origin : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("SceneMappings")
                .AddColumn("SceneOrigin").AsString().Nullable()
                .AddColumn("SearchMode").AsInt32().Nullable()
                .AddColumn("Comment").AsString().Nullable();
        }
    }
}
