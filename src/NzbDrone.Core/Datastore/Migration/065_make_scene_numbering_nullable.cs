using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(65)]
    public class make_scene_numbering_nullable : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE Episodes SET AbsoluteEpisodeNumber = NULL WHERE AbsoluteEpisodeNumber = 0");
            Execute.Sql("UPDATE Episodes SET SceneAbsoluteEpisodeNumber = NULL WHERE SceneAbsoluteEpisodeNumber = 0");
            Execute.Sql("UPDATE Episodes SET SceneSeasonNumber = NULL, SceneEpisodeNumber = NULL WHERE SceneSeasonNumber = 0 AND SceneEpisodeNumber = 0");
        }
    }
}
