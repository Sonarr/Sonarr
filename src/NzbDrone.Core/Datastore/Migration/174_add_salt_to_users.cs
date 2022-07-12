using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(174)]
    public class add_salt_to_users : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Users")
                .AddColumn("Salt").AsString().Nullable()
                .AddColumn("Iterations").AsInt32().Nullable();
        }
    }
}
