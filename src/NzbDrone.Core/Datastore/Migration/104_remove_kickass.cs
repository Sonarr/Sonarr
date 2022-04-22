using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(104)]
    public class remove_kickass : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            IfDatabase("sqlite").Execute.Sql("DELETE FROM Indexers WHERE Implementation = 'KickassTorrents';");
        }
    }
}
