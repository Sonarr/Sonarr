using NzbDrone.Core.Datastore.Migration.Framework;
using FluentMigrator;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(48)]
    public class add_title_to_scenemappings : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("SceneMappings").AddColumn("Title").AsString().Nullable();
        }
    }
}
