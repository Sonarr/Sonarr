using System.Data;
using Migrator.Framework;

namespace NzbDrone.Core.Datastore.Migrations
{
    [Migration(20120123)]
    public class Migration20120123 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.AddColumn("Series", "BacklogStatus", DbType.Int32, ColumnProperty.Null);
            Database.ExecuteNonQuery("UPDATE Series SET BacklogStatus = 2");
        }
    }
}