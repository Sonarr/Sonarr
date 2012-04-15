using System.Data;
using Migrator.Framework;

namespace NzbDrone.Core.Datastore.Migrations
{

    [Migration(20120414)]
    public class Migration20120414 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.AddColumn("History", "Url", DbType.String, ColumnProperty.Null);
        }
    }
}