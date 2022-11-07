using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(17)]
    public class reset_scene_names : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // we were storing new file name as scene name.
            Execute.Sql(@"UPDATE EpisodeFiles SET SceneName = NULL where SceneName != NULL");
        }
    }
}
