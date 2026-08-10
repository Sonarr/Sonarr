using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration;

[Migration(234)]
public class add_next_path_to_series : NzbDroneMigrationBase
{
    protected override void MainDbUpgrade()
    {
        Alter.Table("Series").AddColumn("NextPath").AsString().Nullable();
    }
}
