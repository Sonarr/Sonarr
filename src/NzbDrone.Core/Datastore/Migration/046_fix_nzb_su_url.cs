using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(46)]
    public class fix_nzb_su_url : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE Indexers SET Settings = replace(Settings, '//nzb.su', '//api.nzb.su')" +
                        "WHERE Implementation = 'Newznab'" +
                        "AND Settings LIKE '%//nzb.su%'");
        }
    }
}
