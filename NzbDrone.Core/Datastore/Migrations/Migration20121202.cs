using System;
using System.Data;
using Migrator.Framework;
using NzbDrone.Common;

namespace NzbDrone.Core.Datastore.Migrations
{
    [Migration(20121202)]
    public class Migration20121202 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.ExecuteNonQuery("DELETE FROM Config WHERE [KEY] = 'NewzbinUsername'");
            Database.ExecuteNonQuery("DELETE FROM Config WHERE [KEY] = 'NewzbinPassword'");
            Database.ExecuteNonQuery("DELETE FROM IndexerDefinitions WHERE IndexProviderType = 'NzbDrone.Core.Providers.Indexer.Newzbin'");
        }
    }
}