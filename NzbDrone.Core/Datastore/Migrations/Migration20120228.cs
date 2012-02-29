using System.Data;
using Migrator.Framework;

namespace NzbDrone.Core.Datastore.Migrations
{

    [Migration(20120228)]
    public class Migration20120228 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.AddColumn("Series", "Network", DbType.String, ColumnProperty.Null);
        }
    }
}