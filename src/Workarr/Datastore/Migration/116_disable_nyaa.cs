using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(116)]
    public class disable_nyaa : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"Indexers\" SET \"EnableRss\" = false, \"EnableSearch\" = false, \"Settings\" = Replace(\"Settings\", 'https://nyaa.se', '') WHERE \"Implementation\" = 'Nyaa' AND \"Settings\" LIKE '%nyaa.se%';");
        }
    }
}
