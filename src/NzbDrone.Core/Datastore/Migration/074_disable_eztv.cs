using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(74)]
    public class disable_eztv : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"Indexers\" SET \"EnableRss\" = false, \"EnableSearch\" = false WHERE \"Implementation\" = 'Eztv' AND \"Settings\" LIKE '%ezrss.it%'");
        }
    }
}
