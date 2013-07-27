using System;
using System.Data;
using Migrator.Framework;
using NzbDrone.Common;

namespace NzbDrone.Core.Datastore.Migrations
{
    [Migration(20121227)]
    public class Migration20121227 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.ExecuteNonQuery("UPDATE Config SET [Value] = 'http://update.nzbdrone.com/v1/' WHERE [Key] = 'UpdateUrl'");
        }
    }
}