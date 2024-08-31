using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(212)]
    public class add_role_to_users : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Users").AddColumn("Role").AsString().WithDefaultValue("Admin");
        }
    }
}
