using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(2)]
    public class remove_tvrage_imdb_unique_constraint : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Index().OnTable("Series").OnColumn("TvRageId");
            Delete.Index().OnTable("Series").OnColumn("ImdbId");
        }
    }
}
