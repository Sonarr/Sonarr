using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(240)]
    public class series_title_slug_not_unique : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("DROP INDEX IF EXISTS \"IX_Series_TitleSlug\"");
            Create.Index().OnTable("Series").OnColumn("TitleSlug");
        }
    }
}
