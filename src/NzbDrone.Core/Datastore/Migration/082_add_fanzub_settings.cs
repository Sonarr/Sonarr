using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(82)]
    public class add_fanzub_settings : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE Indexers SET ConfigContract = 'FanzubSettings' WHERE Implementation = 'Fanzub' AND ConfigContract = 'NullConfig'");
        }
    }
}
