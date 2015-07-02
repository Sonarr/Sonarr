using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(90)]
    public class update_kickass_url : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql(
                "UPDATE Indexers SET Settings = Replace(Settings, 'kickass.so', 'kat.cr') WHERE Implementation = 'KickassTorrents';" +
                "UPDATE Indexers SET Settings = Replace(Settings, 'kickass.to', 'kat.cr') WHERE Implementation = 'KickassTorrents';" +
                "UPDATE Indexers SET Settings = Replace(Settings, 'http://', 'https://') WHERE Implementation = 'KickassTorrents';"
           );
        }
    }
}
