using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(116)]
    public class disable_nyaa : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE Indexers SET EnableRss = 0, EnableSearch = 0, Settings = Replace(Settings, 'https://nyaa.se', '') WHERE Implementation = 'Nyaa' AND Settings LIKE '%nyaa.se%';");
        }
    }
}
