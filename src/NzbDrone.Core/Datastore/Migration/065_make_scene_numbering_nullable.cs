using System;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(65)]
    public class make_scene_numbering_nullable : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Update.Table("Episodes").Set(new { AbsoluteEpisodeNumber = DBNull.Value }).Where(new { AbsoluteEpisodeNumber = 0 });
            Update.Table("Episodes").Set(new { SceneAbsoluteEpisodeNumber = DBNull.Value }).Where(new { SceneAbsoluteEpisodeNumber = 0 });
            Update.Table("Episodes").Set(new { SceneSeasonNumber = DBNull.Value, SceneEpisodeNumber = DBNull.Value }).Where(new { SceneSeasonNumber = 0, SceneEpisodeNumber = 0 });
        }
    }
}
