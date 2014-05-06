using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(49)]
    public class fix_dognzb_url : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE Indexers SET Settings = replace(Settings, '//dognzb.cr', '//api.dognzb.cr')" +
                        "WHERE Implementation = 'Newznab'" +
                        "AND Settings LIKE '%//dognzb.cr%'");
        }
    }
}
