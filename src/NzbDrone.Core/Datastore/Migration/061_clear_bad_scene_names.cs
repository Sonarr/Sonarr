using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(61)]
    public class clear_bad_scene_names : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE [EpisodeFiles] " +
                        "SET ReleaseGroup = NULL , SceneName = NULL " +
                        "WHERE " +
                        "   ReleaseGroup IS NULL " +
                        "   OR SceneName IS NULL " +
                        "   OR ReleaseGroup =='DRONE' " +
                        "   OR LENGTH(SceneName) <10 " +
                        "   OR LENGTH(ReleaseGroup) > 20 " +
                        "   OR SceneName NOT LIKE '%.%'");
        }
    }
}
