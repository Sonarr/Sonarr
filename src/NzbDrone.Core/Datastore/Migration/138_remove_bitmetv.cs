using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(138)]
    public class remove_bitmetv : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("DELETE FROM Indexers WHERE Implementation = 'BitMeTv'");

            // Also disable usenet-crawler for the poor guys that still have it enabled
            Execute.Sql("UPDATE Indexers SET EnableRss = 0, EnableAutomaticSearch = 0, EnableInteractiveSearch = 0 WHERE Implementation = 'Newznab' AND Settings LIKE '%usenet-crawler.com%'");
        }
    }
}
