using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(106)]
    public class update_btn_url : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"Indexers\" SET \"Settings\" = Replace(\"Settings\", 'api.btnapps.net', 'api.broadcasthe.net') WHERE \"Implementation\" = 'BroadcastheNet';");
        }
    }

    public class BroadcastheNetSettings106
    {
        public string BaseUrl { get; set; }

        public string ApiKey { get; set; }
    }
}
