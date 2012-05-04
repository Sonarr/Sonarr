using System.Data;
using Migrator.Framework;

namespace NzbDrone.Core.Datastore.Migrations
{

    [Migration(20120504)]
    public class Migration20120504 : NzbDroneMigration
    {
        protected override void MainDbUpgrade()
        {
            Database.AddColumn("NewznabDefinitions", "BuiltIn", DbType.Boolean, ColumnProperty.Null);
        }
    }
}