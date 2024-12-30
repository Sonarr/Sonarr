using FluentMigrator;
using Workarr.Datastore.Migrations.Framework;

namespace Workarr.Datastore.Migrations
{
    [Migration(13)]
    public class add_air_date_utc : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Episodes").AddColumn("AirDateUtc").AsDateTime().Nullable();

            Execute.Sql("UPDATE \"Episodes\" SET \"AirDateUtc\" = \"AirDate\"");
        }
    }
}
