using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(93)]
    public class naming_config_replace_illegal_characters : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("NamingConfig").AddColumn("ReplaceIllegalCharacters").AsBoolean().WithDefaultValue(true);
            Update.Table("NamingConfig").Set(new { ReplaceIllegalCharacters = true }).AllRows();
        }
    }
}
