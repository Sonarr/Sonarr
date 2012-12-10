using System;
using System.Data;
using Migrator.Framework;
using NzbDrone.Common;

namespace NzbDrone.Core.Datastore.Migrations
{
    [Migration(20121209)]
    public class Migration20121209 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.ExecuteNonQuery("DELETE FROM Config WHERE [KEY] = 'NzbMatrixUsername'");
            Database.ExecuteNonQuery("DELETE FROM Config WHERE [KEY] = 'NzbMatrixApiKey'");
            Database.ExecuteNonQuery("DELETE FROM IndexerDefinitions WHERE IndexProviderType = 'NzbDrone.Core.Providers.Indexer.NzbMatrix'");
        }
    }
}