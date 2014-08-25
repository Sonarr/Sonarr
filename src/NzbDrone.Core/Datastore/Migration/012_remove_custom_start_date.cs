using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(12)]
    public class remove_custom_start_date : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("CustomStartDate").FromTable("Series");
        }
    }
}
