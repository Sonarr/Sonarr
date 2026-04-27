using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration;

[Migration(231)]
public class add_tag_existing_to_importlists : NzbDroneMigrationBase
{
    protected override void MainDbUpgrade()
    {
        Alter.Table("ImportLists").AddColumn("TagExisting").AsBoolean().WithDefaultValue(false);
    }
}
