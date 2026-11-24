using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(242)]
    public class add_display_name_column : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("CustomFormats").AddColumn("DisplayName").AsString().Nullable();
        }
    }
}
