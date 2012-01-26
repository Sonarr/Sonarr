using System.Data;
using Migrator.Framework;

namespace NzbDrone.Core.Datastore.Migrations
{
    [Migration(20120123)]
    public class Migration20120123 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.AddColumn("Series", "BacklogSetting", DbType.Int32, ColumnProperty.Null);
        }
    }
}