using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
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
