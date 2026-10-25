using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(240)]
    public class series_title_slug_not_unique : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Index().OnTable("Series").OnColumn("TitleSlug");
            Create.Index().OnTable("Series").OnColumn("TitleSlug");
        }
    }
}
