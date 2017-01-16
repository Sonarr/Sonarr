using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(59)]
    public class add_enable_options_to_indexers : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Indexers")
                 .AddColumn("EnableRss").AsBoolean().Nullable()
                 .AddColumn("EnableSearch").AsBoolean().Nullable();

            Execute.Sql("UPDATE Indexers SET EnableRss = Enable, EnableSearch = Enable");
            Execute.Sql("UPDATE Indexers SET EnableSearch = 0 WHERE Implementation = 'Wombles'");
        }
    }
}
