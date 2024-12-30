using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(206)]
    public class add_tmdbid : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series").AddColumn("TmdbId").AsInt32().WithDefaultValue(0);
            Create.Index().OnTable("Series").OnColumn("TmdbId");
        }
    }
}
