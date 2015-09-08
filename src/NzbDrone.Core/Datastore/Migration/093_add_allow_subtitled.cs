using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(93)]
    public class add_allow_subtitled : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Profiles").AddColumn("AllowSubtitled").AsBoolean().WithDefaultValue(true);
        }
    }
}
