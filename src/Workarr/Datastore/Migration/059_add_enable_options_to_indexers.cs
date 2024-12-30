using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(59)]
    public class add_enable_options_to_indexers : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Indexers")
                 .AddColumn("EnableRss").AsBoolean().Nullable()
                 .AddColumn("EnableSearch").AsBoolean().Nullable();

            Execute.Sql("UPDATE \"Indexers\" SET \"EnableRss\" = \"Enable\", \"EnableSearch\" = \"Enable\"");
            Update.Table("Indexers").Set(new { EnableSearch = false }).Where(new { Implementation = "Wombles" });
        }
    }
}
