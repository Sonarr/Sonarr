using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(105)]
    public class rename_torrent_downloadstation : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE DownloadClients SET Implementation = 'TorrentDownloadStation' WHERE Implementation = 'DownloadStation';");
        }
    }
}
