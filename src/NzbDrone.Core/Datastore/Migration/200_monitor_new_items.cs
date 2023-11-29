using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(200)]
    public class AddNewItemMonitorType : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series").AddColumn("MonitorNewItems").AsInt32().WithDefaultValue(0);
            Alter.Table("ImportLists").AddColumn("MonitorNewItems").AsInt32().WithDefaultValue(0);
        }
    }
}
