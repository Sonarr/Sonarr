using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(153)]
    public class add_on_episodefiledelete_for_upgrade : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Notifications").AddColumn("OnEpisodeFileDeleteForUpgrade").AsBoolean().WithDefaultValue(1);
        }
    }
}
