using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(107)]
    public class remove_wombles : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            IfDatabase("sqlite").Execute.Sql("DELETE FROM Indexers WHERE Implementation = 'Wombles';");
        }
    }
}
