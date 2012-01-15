using System.Data;
using Migrator.Framework;

namespace NzbDrone.Core.Datastore.Migrations
{

    [Migration(20111125)]
    public class Migration2011125 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.AddColumn("Series", "IsDaily", DbType.Boolean, ColumnProperty.Null);
        }
    }
}