using System.Data;
using Migrator.Framework;

namespace NzbDrone.Core.Datastore.Migrations
{

    [Migration(20120430)]
    public class Migration20120430 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.AddColumn("History", "NzbInfoUrl", DbType.String, ColumnProperty.Null);
        }
    }
}