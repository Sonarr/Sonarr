using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(13)]
    public class add_air_date_utc : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Episodes").AddColumn("AirDateUtc").AsDateTime().Nullable();

            Execute.Sql("UPDATE Episodes SET AirDateUtc = AirDate");
        }
    }
}
