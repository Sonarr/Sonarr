using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(230)]
    public class add_mapping_id_to_scene_mappings : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("SceneMappings")
                 .AddColumn("MappingId").AsString().Nullable();
        }
    }
}
