using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(203)]
    public class add_add_release_hash : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("EpisodeFiles").AddColumn("ReleaseHash").AsString().Nullable();
        }
    }
}
