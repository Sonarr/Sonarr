using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(14)]
    public class drop_air_date : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("AirDate").FromTable("Episodes");
        }
    }
}
