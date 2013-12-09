using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(32)]
    public class set_default_release_group : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE EpisodeFiles SET ReleaseGroup = 'DRONE' WHERE ReleaseGroup IS NULL");
        }
    }
}
