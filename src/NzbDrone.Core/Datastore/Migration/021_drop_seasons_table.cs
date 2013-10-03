using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(21)]
    public class drop_seasons_table : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Table("Seasons");
        }
    }
}
