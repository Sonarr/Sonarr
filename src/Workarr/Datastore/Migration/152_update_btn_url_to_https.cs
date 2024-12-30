using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(152)]
    public class update_btn_url_to_https : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"Indexers\" SET \"Settings\" = Replace(\"Settings\", 'http://api.broadcasthe.net', 'https://api.broadcasthe.net') WHERE \"Implementation\" = 'BroadcastheNet';");
        }
    }
}
