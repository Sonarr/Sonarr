using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(32)]
    public class set_default_release_group : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Update.Table("EpisodeFiles").Set(new { ReleaseGroup = "DRONE" }).Where(new { ReleaseGroup = DBNull.Value });
        }
    }
}
