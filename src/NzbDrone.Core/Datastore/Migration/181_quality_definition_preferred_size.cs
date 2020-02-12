using System.Data;
using Dapper;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(181)]
    public class quality_definition_preferred_size : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("QualityDefinitions").AddColumn("PreferredSize").AsDouble().Nullable();

            Execute.WithConnection(UpdateQualityDefinitions);
        }

        private void UpdateQualityDefinitions(IDbConnection conn, IDbTransaction tran)
        {
            var updateSql = "UPDATE QualityDefinitions SET PreferredSize = MaxSize - 5 WHERE MaxSize > 5";
            conn.Execute(updateSql, transaction: tran);
        }
    }
}
