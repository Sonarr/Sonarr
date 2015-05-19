using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(84)]
    public class update_quality_minmax_size : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("QualityDefinitions").AlterColumn("MinSize").AsDouble().Nullable();
            Alter.Table("QualityDefinitions").AlterColumn("MaxSize").AsDouble().Nullable();

            Execute.Sql("UPDATE QualityDefinitions SET MaxSize = NULL WHERE Quality = 10 OR MaxSize = 0");
        }
    }
}
