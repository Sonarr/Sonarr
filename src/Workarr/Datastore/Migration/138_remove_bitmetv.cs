using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(138)]
    public class remove_bitmetv : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.FromTable("Indexers").Row(new { Implementation = "BitMeTv" });

            // Also disable usenet-crawler for the poor guys that still have it enabled
            Execute.Sql("UPDATE \"Indexers\" SET \"EnableRss\" = false, \"EnableAutomaticSearch\" = false, \"EnableInteractiveSearch\" = false WHERE \"Implementation\" = 'Newznab' AND \"Settings\" LIKE '%usenet-crawler.com%'");
        }
    }
}
