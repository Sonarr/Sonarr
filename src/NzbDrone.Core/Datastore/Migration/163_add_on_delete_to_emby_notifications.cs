using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(163)]
    public class add_on_delete_to_emby_notifications : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE Notifications SET OnSeriesDelete = 1 WHERE Implementation = 'MediaBrowser'");
            Execute.Sql("UPDATE Notifications SET OnEpisodeFileDelete = 1 WHERE Implementation = 'MediaBrowser'");
            Execute.Sql("UPDATE Notifications SET OnEpisodeFileDeleteForUpgrade = 1 WHERE Implementation = 'MediaBrowser'");
        }
    }
}
