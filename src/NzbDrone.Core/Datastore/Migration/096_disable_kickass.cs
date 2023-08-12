using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(96)]
    public class disable_kickass : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"Indexers\" SET \"EnableRss\" = false, \"EnableSearch\" = false, \"Settings\" = Replace(\"Settings\", 'https://kat.cr', '') WHERE \"Implementation\" = 'KickassTorrents' AND \"Settings\" LIKE '%kat.cr%';");
        }
    }
}
