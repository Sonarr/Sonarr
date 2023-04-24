using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(189)]
    public class add_colon_replacement_to_naming_config : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig").AddColumn("ColonReplacementFormat").AsInt32().WithDefaultValue(4);
        }
    }
}
