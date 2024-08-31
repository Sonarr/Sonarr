using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(213)]
    public class add_api_key_to_users : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Users").AddColumn("ApiKey").AsString().WithDefaultValue("Test");
        }
    }
}
