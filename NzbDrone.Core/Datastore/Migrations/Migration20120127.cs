using System.Data;
using Migrator.Framework;

namespace NzbDrone.Core.Datastore.Migrations
{
    [Migration(20120127)]
    public class Migration20120127 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.AddColumn("History", "NewzbinId", DbType.Int32, ColumnProperty.Null);
            Database.AddColumn("History", "Blacklisted", DbType.Boolean, ColumnProperty.Null);
        }
    }
}